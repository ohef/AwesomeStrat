using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class WhereToMoveState : BattleState
{
    private Unit SelectedUnit;
    private Vector2Int InitialUnitPosition;
    private MapDecorator Decorator;
    private HashSet<Vector2Int> MovementTiles;
    private LinkedList<Vector2Int> PointsToPass;

    public void Initialize( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
        InitialUnitPosition = sys.Map.UnitPos[ SelectedUnit ];
        Decorator = sys.Map.GetComponent<MapDecorator>();
    }

    public void OnEnable()
    {
        sys.Cursor.CursorMoved.AddListener( CursorMoved );

        PointsToPass = new LinkedList<Vector2Int>();
        PointsToPass.AddFirst( InitialUnitPosition );
        MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit ) );
        EventSystem.current.SetSelectedGameObject( gameObject );
    }

    public void Update()
    {
        Unit unitUnderCursor = null;
        sys.Map.UnitPos.TryGetValue( sys.Cursor.CurrentPosition, out unitUnderCursor );

        Decorator.RenderForPath( PointsToPass );
        if ( Input.GetButtonDown( "Submit" ) )
        {
            bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentPosition );
            if ( canMoveHere )
            {
                Context.DoCommand(
                    sys.CreateMoveCommand( PointsToPass.ToList(), SelectedUnit ) );

                var state = sys.GetState<ChoosingUnitActionsState>();
                state.Initialize( SelectedUnit );
                Context.State = state;
            }
        }
    }

    public void OnDisable()
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ SelectedUnit ] );
        PointsToPass.Clear();
        Decorator.RenderForPath( PointsToPass );
    }

    private void CursorMoved()
    {
        bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentPosition );
        if ( withinMoveRange )
        {
            LengthenMovementPath( sys.Cursor.CurrentPosition );
            Decorator.RenderForPath( PointsToPass );
        }
    }

    private void LengthenMovementPath( Vector2Int to )
    {
        bool tooFarFromLast = false;
        if ( PointsToPass.Count > 0 )
            tooFarFromLast = PointsToPass.Last.Value
                .ManhattanDistance( to ) > 1;

        if ( PointsToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
        {
            PointsToPass = new LinkedList<Vector2Int>(
                MapSearcher.Search( InitialUnitPosition, to,
                MapSearcher.CalculateNodeMap( sys.Map, InitialUnitPosition, SelectedUnit.MovementRange ) ) );
            return;
        }

        LinkedListNode<Vector2Int> alreadyPresent = PointsToPass.Find( to );
        if ( alreadyPresent == null )
        {
            PointsToPass.AddLast( to );
        }
        else
        {
            while ( PointsToPass.Last != alreadyPresent )
            {
                PointsToPass.RemoveLast();
            }
        }
    }
}