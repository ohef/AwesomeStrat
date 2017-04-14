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
    }

    public void CursorMoved()
    {
        if ( this.gameObject.activeSelf )
        {
            Decorator.ShowUnitMovement( sys.Cursor.GetCurrentUnit() );
        }
    }
}