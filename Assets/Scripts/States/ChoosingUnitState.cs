using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosingUnitState : ControlCursorState
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
        base.Update( sys );
        MapUnit unitAtTile;
        if ( sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitAtTile ) )
        {
            if ( Input.GetButtonDown( "Submit" ) && unitAtTile.hasTakenAction == false )
            {
                Animator unitAnimator = unitAtTile.GetComponentInChildren<Animator>();
                unitAnimator.SetBool( "Selected", true );
                sys.TurnState = new ChoosingUnitActionsState( unitAtTile );
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

