using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class ControlCursorState : BattleState, IPointerDownHandler
{
    public void OnMove( AxisEventData eventData )
    {
        sys.Cursor.ShiftCursor( eventData.moveVector.ToVector2Int() );
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        GameObject obj = eventData.pointerPressRaycast.gameObject;
        OnPointerDown( obj.GetComponent<GameTile>() );
        OnPointerDown( obj.GetComponent<Unit>() );
    }

    void OnPointerDown( GameTile tile )
    {
        if ( tile != null )
            sys.Cursor.MoveCursor( sys.Map.TilePos[ tile ] );
    }

    void OnPointerDown( Unit unit )
    {
        if ( unit != null )
            sys.Cursor.MoveCursor( sys.Map.UnitPos[ unit ] );
    }
}