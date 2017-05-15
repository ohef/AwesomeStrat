using Assets.General.UnityExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControlCursorState : BattleState, IMoveHandler
{
    public void OnMove( AxisEventData eventData )
    {
        sys.Cursor.ShiftCursor( eventData.moveVector.ToVector2Int() );
    }
}