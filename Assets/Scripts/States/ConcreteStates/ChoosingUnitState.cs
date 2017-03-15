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
                Animator unitAnimator = unitAtTile.GetComponentInChildren<Animator>();
                unitAnimator.SetTrigger( "Selected" );
                StartMoving( unitAtTile );
            }
            else
            {
                sys.TurnState = TurnMenuState.Create();
            }
        }
    }

    private void StartMoving( Unit unit )
    {
        sys.TurnState = WhereToMoveState.Create( unit );
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
        sys.Map.ShowUnitMovementIfHere( sys.Cursor.CurrentTile );
    }
}

