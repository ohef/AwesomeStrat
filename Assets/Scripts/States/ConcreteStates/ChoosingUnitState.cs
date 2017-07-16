﻿using Assets.General.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

sealed class ChoosingUnitState : BattleState
    , ISubmitHandler, IPointerDownHandler
{
    MapDecorator Decorator;

    public void Start()
    {
        Decorator = sys.Map.GetComponent<MapDecorator>();
    }

    public void CursorMoved()
    {
        if ( Decorator != null )
            Decorator.ShowUnitMovement( sys.Cursor.GetCurrentUnit() );
    }

    public override void Enter()
    {
        sys.Cursor.CursorMoved.AddListener( CursorMoved );
    }

    public override void Exit()
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
    }

    public void SelectUnit( Unit unit )
    {
        Transition<WhereToMoveState>( state => state.Initialize( unit ) );
    }

    private bool IsUnitAtTileMine( Vector2Int pos, out Unit unitAtTile )
    {
        var player = sys.CurrentTurn;
        return sys.Map.UnitPos.TryGetValue( pos, out unitAtTile )
                    && player.ControlledUnits.Contains( unitAtTile )
                    && player.HasNotActed.Contains( unitAtTile );
    }

    #region Handlers
    public void OnSubmit( BaseEventData eventData )
    {
        Unit unitAtTile;
        if ( IsUnitAtTileMine( sys.Cursor.CurrentPosition, out unitAtTile ) )
        {
            SelectUnit( unitAtTile );
        }
    }

    public void OnPointerDown( PointerEventData eventData, Unit unit )
    {
        Transition<WhereToMoveState>( state => state.Initialize( unit ) );
        Decorator.ShowUnitMovement( unit );
    }

    public void OnPointerDown( PointerEventData eventData, GameTile tile ) { }

    public void OnPointerDown( PointerEventData eventData )
    {
        Debug.Log( "Pointer Down" );
        var unit = eventData.pointerPressRaycast.gameObject.GetComponent<Unit>();
        if ( unit != null )
        {
            OnPointerDown( eventData, unit );
        }
    }
    #endregion
}