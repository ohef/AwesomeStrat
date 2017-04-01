using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Rendering;

public class WhereToMoveState : BattleState
{
    private Unit SelectedUnit;
    private GameTile InitialUnitTile;
    private MapDecorator Decorator;
    private HashSet<Vector2Int> MovementTiles;
    private LinkedList<GameTile> TilesToPass;

    public static BattleState Create( Unit selectedUnit )
    {
        return new ControlCursorState( new CancelableState( new WhereToMoveState( selectedUnit ) ) );
    }

    private WhereToMoveState( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
        InitialUnitTile = sys.Map.UnitGametileMap[ SelectedUnit ];
        Decorator = sys.Map.GetComponent<MapDecorator>();
    }

    public override void Update( PlayerTurnController context )
    {
        Unit unitUnderCursor = null;
        sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitUnderCursor );

        Decorator.RenderForPath( TilesToPass );
        if ( Input.GetButtonDown( "Submit" ) )
        {
            bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
            if ( canMoveHere )
            {
                context.DoCommand( 
                    CreateMoveCommand( sys.Cursor.CurrentTile, InitialUnitTile ) );
                sys.TurnState = ChoosingUnitActionsState.Create( SelectedUnit );
            }
        }
    }

    public override void Enter( PlayerTurnController context )
    {
        sys.Cursor.CursorMoved.AddListener( CursorMoved );

        TilesToPass = new LinkedList<GameTile>();
        TilesToPass.AddFirst( InitialUnitTile );
        MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, InitialUnitTile ) );
    }

    public override void Exit( PlayerTurnController context )
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        TilesToPass.Clear();
        Decorator.RenderForPath( TilesToPass );
    }

    private UndoCommandAction CreateMoveCommand( GameTile targetTile, GameTile initialTile )
    {
        return new UndoCommandAction(
            delegate
            {
                SelectedUnit.StartCoroutine(
                    CoroutineHelper.AddActions( 
                        CustomAnimation.InterpolateBetweenPointsDecoupled( SelectedUnit.transform,
                        SelectedUnit.transform.FindChild( "Model" ),
                        TilesToPass.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(), 0.22f ),
                        () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", true ),
                        () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false ) ) );

                sys.Map.PlaceUnit( SelectedUnit, targetTile );
            },
            delegate
            {
                SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false );
                SelectedUnit.StopAllCoroutines();

                sys.Map.PlaceUnit( SelectedUnit, initialTile );
                SelectedUnit.transform.position = initialTile.transform.position;

                sys.Cursor.MoveCursor( initialTile.Position );
                sys.Map.GetComponent<MapDecorator>().ShowUnitMovement( SelectedUnit );
            } );
    }

    private void CursorMoved()
    {
        bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
        if ( withinMoveRange )
        {
            AttemptToLengthenPath( sys.Cursor.CurrentTile );
            Decorator.RenderForPath( TilesToPass );
        }
    }

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