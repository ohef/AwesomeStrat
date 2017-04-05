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
                    sys.CreateMoveCommand( new LinkedList<GameTile>( TilesToPass ), SelectedUnit ) );
                context.State = ChoosingUnitActionsState.Create( SelectedUnit );
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

    private void CursorMoved()
    {
        bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
        if ( withinMoveRange )
        {
            LengthenMovementPath( sys.Cursor.CurrentTile );
            Decorator.RenderForPath( TilesToPass );
        }
    }

    private void LengthenMovementPath( GameTile to )
    {
        bool tooFarFromLast = false;
        if ( TilesToPass.Count > 0 )
            tooFarFromLast = TilesToPass.Last.Value.Position
                .ManhattanDistance( to.Position ) > 1;

        if ( TilesToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
        {
            TilesToPass = new LinkedList<GameTile>( MapSearcher.Search( InitialUnitTile, to, sys.Map, SelectedUnit.MovementRange ) );
            return;
        }

        LinkedListNode<GameTile> alreadyPresent = TilesToPass.Find( to );
        if ( alreadyPresent == null )
        {
            TilesToPass.AddLast( to );
        }
        else
        {
            while ( TilesToPass.Last != alreadyPresent )
            {
                TilesToPass.RemoveLast();
            }
        }
    }
}