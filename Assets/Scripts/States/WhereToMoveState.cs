using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class WhereToMoveState : ControlCursorState
{
    private MapUnit SelectedUnit;
    private GameTile InitialUnitTile;
    private HashSet<Vector2Int> MovementTiles;
    private LinkedList<GameTile> TilesToPass;

    public WhereToMoveState( MapUnit selectedUnit )
    {
        SelectedUnit = selectedUnit;
        InitialUnitTile = sys.Map.UnitGametileMap[ SelectedUnit ];
    }

    public override void Update( BattleSystem sys )
    {
        base.Update( sys );
        bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );

        MapUnit unitUnderCursor = null;
        sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitUnderCursor );

        if ( Input.GetButtonDown( "Submit" ) )
        {
            bool notTheSameUnit = unitUnderCursor != SelectedUnit;
            if ( canMoveHere && notTheSameUnit )
            {
                sys.CurrentTurn.DoCommand( 
                    CreateMoveCommand( sys.Cursor.CurrentTile, InitialUnitTile ) );
                sys.TurnState = new ChoosingUnitActionsState( SelectedUnit );
            }
        }
    }

    private UndoCommandAction CreateMoveCommand( GameTile targetTile, GameTile initialTile )
    {
        return new UndoCommandAction(
            delegate
            {
                SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", true );

                SelectedUnit.StartCoroutine(
                    CoroutineHelper.AddAfter( CustomAnimation.InterpolateBetweenPoints( SelectedUnit.transform,
                    TilesToPass.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(),
                    0.22f ),
                () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false ) ) );

                sys.Map.PlaceUnit( SelectedUnit, targetTile );
                SelectedUnit.hasMoved = true;
            },
            delegate
            {
                SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false );
                SelectedUnit.StopAllCoroutines();

                sys.Map.PlaceUnit( SelectedUnit, initialTile );
                SelectedUnit.transform.position = initialTile.transform.position;

                sys.Cursor.MoveCursor( initialTile.Position );
                SelectedUnit.hasMoved = false;
            } );
    }

    public override void Enter( BattleSystem sys )
    {
        sys.InRenderObject += OnRenderObject;
        sys.Cursor.CursorMoved.AddListener( CursorMoved );
        TilesToPass = new LinkedList<GameTile>();
        TilesToPass.AddFirst( InitialUnitTile );
        MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, InitialUnitTile ) );
    }

    public override void Exit( BattleSystem sys )
    {
        sys.InRenderObject -= OnRenderObject;
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
    }

    public void CursorMoved()
    {
        bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
        if ( withinMoveRange )
        {
            AttemptToLengthenPath( sys.Cursor.CurrentTile );
        }
    }

    public void OnRenderObject()
    {
        sys.Map.RenderForPath( TilesToPass );
    }

    //private void ExecuteAttack( MapUnit selectedUnit, MapUnit unitUnderCursor )
    //{
    //    GameTile lastTile = TilesToPass.First();
    //    if ( lastTile != SelectedTile ) //We need to move
    //    {
    //        ExecuteMove( lastTile );
    //    }
    //    selectedUnit.AttackUnit( unitUnderCursor );
    //}

    //private GameTile GetOptimalAttackPosition( GameTile on )
    //{
    //    // Does a reverse lookup from position to see if there are any 
    //    // tiles we can move around the point we can move to
    //    IEnumerable<Vector2Int> canMovePositions = sys.Map
    //        .GetTilesWithinAbsoluteRange( on.Position, SelectedUnit.AttackRange )
    //        .Where( pos => MovementTiles.Contains( pos ) )
    //        .ToList();

    //    if ( canMovePositions.Count() == 0 )
    //        return null;
    //    else if ( canMovePositions.Any( pos => pos == SelectedTile.Position ) )
    //        return SelectedTile;

    //    Vector2Int optimalPosition = canMovePositions
    //        .Select( pos => new { Pos = pos, Distance = on.Position.ManhattanDistance( pos ) } )
    //        .OrderByDescending( data => data.Distance )
    //        .First()
    //        .Pos;

    //    return sys.Map[ optimalPosition ];
    //}

    private void AttemptToLengthenPath( GameTile to )
    {
        bool tooFarFromLast = false;
        if ( TilesToPass.Count > 0 )
            tooFarFromLast = TilesToPass.First.Value.Position
                .ManhattanDistance( to.Position ) > 1;

        if ( TilesToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
        {
            TilesToPass = new LinkedList<GameTile>( MapSearcher.Search( InitialUnitTile, to, sys.Map, SelectedUnit.MovementRange ) );
            return;
        }

        LinkedListNode<GameTile> alreadyPresent = TilesToPass.Find( to );
        if ( alreadyPresent == null )
        {
            TilesToPass.AddFirst( to );
        }
        else
        {
            while ( TilesToPass.First != alreadyPresent )
            {
                TilesToPass.RemoveFirst();
            }
        }
    }
}

