﻿using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

sealed class WhereToMoveState : BattleState, ISubmitHandler
    , IBeginDragHandler, IDragHandler, IEndDragHandler
    , IPointerDownHandler, IPointerClickHandler 
{
    private Unit SelectedUnit;
    private Vector2Int InitialUnitPosition;
    private MapDecorator Decorator;
    private HashSet<Vector2Int> MovementTiles;
    private LinkedList<Vector2Int> PointsToPass;

    public WhereToMoveState Initialize( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
        InitialUnitPosition = sys.Map.UnitPos[ SelectedUnit ];
        Decorator = sys.Map.GetComponent<MapDecorator>();
        return this;
    }

    private void CursorMoved()
    {
        bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentPosition );
        if ( withinMoveRange )
        {
            LengthenMovementPath( sys.Cursor.CurrentPosition );
            Decorator.RenderMovePath( PointsToPass );
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
                MapSearcher.Search( 
                    InitialUnitPosition, to, 
                    sys.Map, SelectedUnit.MovementRange ) );
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

    public void OnSubmit( BaseEventData eventData )
    {
        bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentPosition );
        if ( canMoveHere )
        {
            sys.DoCommand(
                sys.CreateMoveCommand( PointsToPass.ToList(), SelectedUnit ) );

            Transition<ChoosingUnitActionsState>(
                state => state.Initialize( SelectedUnit ) );
        }
    }

    public override void Enter()
    {
        sys.Cursor.CursorMoved.AddListener( CursorMoved );

        PointsToPass = new LinkedList<Vector2Int>();
        PointsToPass.AddFirst( InitialUnitPosition );
        MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit ) );
        base.Enter();
    }

    public override void Exit()
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
        PointsToPass.Clear();
        Decorator.RenderMovePath( PointsToPass );
        base.Exit();
    }

    public void OnPointerClick( Unit unit, PointerEventData eventData ) { }

    public void OnPointerClick( GameTile tile, PointerEventData eventData )
    {
        if ( tile == null ) return;

        //Cancel the state if we are hitting somewhere other than where we can move
        if ( MovementTiles.Contains( sys.Map.TilePos[ tile ] ) == false )
            ExecuteEvents.Execute( gameObject, null, ExecuteEvents.cancelHandler );
        else if ( eventData.clickCount == 2 )
        {
            OnSubmit( eventData );
        }
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        //GameObject obj = eventData.pointerPressRaycast.gameObject;
        //OnPointerDown( obj.GetComponent<GameTile>(), eventData );
        //OnPointerDown( obj.GetComponent<Unit>(), eventData );
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        GameObject obj = eventData.pointerPressRaycast.gameObject;
        OnPointerClick( obj.GetComponent<GameTile>(), eventData );
        OnPointerClick( obj.GetComponent<Unit>(), eventData );
    }

    //UnitMemento BeforeDrag;
    Unit DraggingUnit;

    public void OnBeginDrag( PointerEventData eventData )
    {
        DraggingUnit = eventData.pointerCurrentRaycast.gameObject.GetComponent<Unit>();
        DraggingUnit = DraggingUnit == SelectedUnit ? DraggingUnit : null;
    }

    public void OnDrag( PointerEventData eventData )
    {
        if ( DraggingUnit != null )
        {
            DraggingUnit.transform.position = eventData.pointerCurrentRaycast.worldPosition;

            List<RaycastResult> result = new List<RaycastResult>();

            Camera.main.GetComponent<PhysicsRaycaster>().Raycast( eventData, result );

            sys.Cursor.MoveCursor( sys.Map.TilePos[
                result.First( val => val.gameObject.GetComponent<GameTile>() != null )
                .gameObject.GetComponent<GameTile>() ] );
        }
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        if ( DraggingUnit != null )
        {
            //Validate movement position
            if ( CurrentCursorPositionIsValid() )
            {
                sys.DoCommand( sys.CreateMoveCommand( sys.Cursor.CurrentPosition, SelectedUnit ) );
                Transition<ChoosingUnitActionsState>( state => state.Initialize( SelectedUnit ) );
            }
            else
            {
                StartCoroutine( CustomAnimation.InterpolateToPoint( DraggingUnit.transform, InitialUnitPosition.ToVector3(), 0.25f ) );
            }
        }
    }

    private bool CurrentCursorPositionIsValid()
    {
        var cursorPosition = sys.Cursor.CurrentPosition;
        return
            cursorPosition != InitialUnitPosition &&
            sys.Map.Occupied( cursorPosition ) == false &&
            MovementTiles.Contains( cursorPosition );
    }
}