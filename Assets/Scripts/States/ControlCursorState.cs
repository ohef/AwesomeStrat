using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlCursorState : BattleState
{
    public override void Update( BattleSystem sys )
    {
        if ( Input.GetButtonDown( "Cancel" ) )
            sys.CurrentTurn.GoToPreviousState();

        sys.Cursor.UpdateAction();
    }
}