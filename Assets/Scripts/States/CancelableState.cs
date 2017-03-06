using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelableState : BattleState {
    // Update is called once per frame
    public override void Update( BattleSystem sys )
    {
        if ( Input.GetButtonDown( "Cancel" ) )
        {
            Input.ResetInputAxes();
            sys.CurrentTurn.UndoEverything();
            sys.CurrentTurn.GoToPreviousState();
        }
	}
}
