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

    public override void Update( BattleSystem sys )
    {
        if ( Input.GetButtonDown( "Submit" ) )
        {
            Unit unitAtTile;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitAtTile ) 
                && sys.CurrentTurn.ControlledUnits.Contains( unitAtTile )
                && sys.CurrentTurn.HasNotActed.Contains( unitAtTile ) )
            {
                Animator unitAnimator = unitAtTile.GetComponentInChildren<Animator>();
                unitAnimator.SetBool( "Selected", true );
                sys.TurnState = ChoosingUnitActionsState.Create( unitAtTile ); 
            }
            else
            {
                sys.TurnState = TurnMenuState.Create();
            }
        }
    }

    public override void Enter( BattleSystem sys )
    {
        sys.Cursor.CursorMoved.AddListener( CursorMoved );
    }

    public override void Exit( BattleSystem sys )
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
    }

    public void CursorMoved()
    {
        sys.Map.ShowUnitMovementIfHere( sys.Cursor.CurrentTile );
    }
}

