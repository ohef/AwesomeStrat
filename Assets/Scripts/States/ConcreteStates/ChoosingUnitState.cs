using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChoosingUnitState : BattleState, ISubmitHandler
{
    MapDecorator Decorator; 

    public void OnEnable()
    {
        Decorator = sys.Map.GetComponent<MapDecorator>();
        EventSystem.current.SetSelectedGameObject( gameObject );
    }

    public void CursorMoved()
    {
        if ( this.gameObject.activeSelf )
        {
            Decorator.ShowUnitMovement( sys.Cursor.GetCurrentUnit() );
        }
    }

    public void OnSubmit( BaseEventData eventData )
    {
        Unit unitAtTile;
        var context = sys.CurrentTurn as PlayerTurnController;
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
        eventData.Use();
    }
}