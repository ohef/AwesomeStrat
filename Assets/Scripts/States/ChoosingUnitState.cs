using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosingUnitState : BattleState
{
    private static ChoosingUnitState instance = new ChoosingUnitState();
    public static ChoosingUnitState Instance
    {
        get
        {
            return instance;
        }
    }

    public override void Update( BattleSystem sys )
    {
        sys.Cursor.UpdateAction();
        if ( Input.GetButtonDown( "Submit" ) )
        {
            Unit unitAtTile;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitAtTile ) 
                && sys.CurrentTurn.ControlledUnits.Contains( unitAtTile )
                && sys.CurrentTurn.HasNotActed.Contains( unitAtTile ) )
            {
                Animator unitAnimator = unitAtTile.GetComponentInChildren<Animator>();
                unitAnimator.SetBool( "Selected", true );
                sys.TurnState = new ChoosingUnitActionsState( unitAtTile );
            }
            else
            {
                sys.TurnState = new TurnMenuState();
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

