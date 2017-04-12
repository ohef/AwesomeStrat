using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosingUnitState : BattleState
{
    private static BattleState instance = new ControlCursorState( new ChoosingUnitState() );
    public static BattleState Instance
    {
        get
        {
            return instance;
        }
    }
    MapDecorator Decorator; 

    public override void Update( PlayerTurnController context )
    {
        if ( Input.GetButtonDown( "Submit" ) )
        {
            Unit unitAtTile;
            if ( sys.Map.UnitPos.TryGetValue( sys.Cursor.CurrentPosition, out unitAtTile )
                && context.ControlledUnits.Contains( unitAtTile )
                && context.HasNotActed.Contains( unitAtTile ) )
            {
                context.State = WhereToMoveState.Create( unitAtTile );
            }
            else
            {
                context.State = TurnMenuState.Create();
            }
        }
    }

    public override void Enter( PlayerTurnController context )
    {
        Decorator = sys.Map.GetComponent<MapDecorator>();
        sys.Cursor.CursorMoved.AddListener( CursorMoved );
    }

    public override void Exit( PlayerTurnController context )
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
    }

    public void CursorMoved()
    {
        Decorator.ShowUnitMovement( sys.Cursor.GetCurrentUnit() );
    }
}

