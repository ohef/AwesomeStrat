using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelableState : BattleState
{
    public bool undo;

    // Update is called once per frame
    public void Update()
    {
        if ( Input.GetButtonDown( "Cancel" ) )
        {
            Input.ResetInputAxes();
            if ( undo == true )
                Context.UndoEverything();
            Context.GoToPreviousState();
        }
	}
}
