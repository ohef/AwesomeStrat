using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosingUnitState : BattleState
{
    MapDecorator Decorator; 

    public void Update()
    {
        var context = sys.CurrentTurn as PlayerTurnController;
        if ( Input.GetButtonDown( "Submit" ) )
        {
            Unit unitAtTile;
            if ( sys.Map.UnitPos.TryGetValue( sys.Cursor.CurrentPosition, out unitAtTile )
                && context.ControlledUnits.Contains( unitAtTile )
                && context.HasNotActed.Contains( unitAtTile ) )
            {
                var state = sys.GetState<WhereToMoveState>();
                state.Initialize( unitAtTile );
                context.State = state;
            }
            else
            {
                var state = sys.GetState<TurnMenuState>();
                context.State = state;
            }
        }
    }

    public void OnEnable()
    {
        Decorator = sys.Map.GetComponent<MapDecorator>();
        sys.Cursor.CursorMoved.AddListener( CursorMoved );
    }

    public void OnDisable()
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
    }

    public void CursorMoved()
    {
        Decorator.ShowUnitMovement( sys.Cursor.GetCurrentUnit() );
    }
}