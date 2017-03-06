using Assets.General.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhereToMoveState : ControlCursorState
{
    private MapUnit SelectedUnit;
    private GameTile SelectedTile { get { return sys.Map.UnitGametileMap[ SelectedUnit ]; } }
    private HashSet<Vector2Int> MovementTiles;
    private LinkedList<GameTile> TilesToPass;

    public WhereToMoveState( MapUnit selectedUnit )
    {
        SelectedUnit = selectedUnit;
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
                ExecuteMove( sys.Cursor.CurrentTile );
                sys.TurnState = new ChoosingUnitActionsState( SelectedUnit );
            }
        }
    }

    public override void Enter( BattleSystem sys )
    {
        sys.InRenderObject += OnRenderObject;
        sys.Cursor.CursorMoved.AddListener( CursorMoved );
        TilesToPass = new LinkedList<GameTile>();
        TilesToPass.AddFirst( SelectedTile );
        MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, SelectedTile ) );
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

    private void ExecuteAttack( MapUnit selectedUnit, MapUnit unitUnderCursor )
    {
        GameTile lastTile = TilesToPass.First();
        if ( lastTile != SelectedTile ) //We need to move
        {
            ExecuteMove( lastTile );
        }
        selectedUnit.AttackUnit( unitUnderCursor );
    }

    private GameTile GetOptimalAttackPosition( GameTile on )
    {
        // Does a reverse lookup from position to see if there are any 
        // tiles we can move around the point we can move to
        IEnumerable<Vector2Int> canMovePositions = sys.Map
            .GetTilesWithinAbsoluteRange( on.Position, SelectedUnit.AttackRange )
            .Where( pos => MovementTiles.Contains( pos ) )
            .ToList();

        if ( canMovePositions.Count() == 0 )
            return null;
        else if ( canMovePositions.Any( pos => pos == SelectedTile.Position ) )
            return SelectedTile;

        Vector2Int optimalPosition = canMovePositions
            .Select( pos => new { Pos = pos, Distance = on.Position.ManhattanDistance( pos ) } )
            .OrderByDescending( data => data.Distance )
            .First()
            .Pos;

        return sys.Map[ optimalPosition ];
    }

    private void ExecuteMove( GameTile to )
    {
        SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", true );

        sys.StartCoroutine(
            General.UnityExtensions.CoroutineHelper.AddAfter(
                General.CustomAnimation.InterpolateBetweenPoints( SelectedUnit.transform,
                TilesToPass.Select( x => x.GetComponent<Transform>().localPosition )
                .Reverse().ToList(), 0.22f ),
                () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false ) ) );

        sys.Map.SwapUnit( SelectedTile, to );
        SelectedUnit.hasMoved = true;
    }

    private void AttemptToLengthenPath( GameTile to )
    {
        bool tooFarFromLast = false;
        if ( TilesToPass.Count > 0 )
            tooFarFromLast = TilesToPass.First.Value.Position
                .ManhattanDistance( to.Position ) > 1;

        if ( TilesToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
        {
            TilesToPass = new LinkedList<GameTile>( MapSearcher.Search( SelectedTile, to, sys.Map, SelectedUnit.MovementRange ) );
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

