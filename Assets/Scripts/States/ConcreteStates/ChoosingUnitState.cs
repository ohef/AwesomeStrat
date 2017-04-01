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

    public override void Update( TurnState context )
    {
        if ( Input.GetButtonDown( "Submit" ) )
        {
            Unit unitAtTile;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitAtTile ) 
                && context.ControlledUnits.Contains( unitAtTile )
                && context.HasNotActed.Contains( unitAtTile ) )
            {
                sys.TurnState = WhereToMoveState.Create( unitAtTile );
            }
            else
            {
                sys.TurnState = TurnMenuState.Create();
            }
        }
    }

    public override void Enter( TurnState context )
    {
        sys.Cursor.CursorMoved.AddListener( CursorMoved );
    }

    public override void Exit( TurnState context )
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
    }

    public void CursorMoved()
    {
        sys.Map.GetComponent<MapDecorator>().ShowUnitMovement( sys.Cursor.GetCurrentUnit() );
    }
}

