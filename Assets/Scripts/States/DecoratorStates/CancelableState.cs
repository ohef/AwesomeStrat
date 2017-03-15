using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelableState : DecoratorState
{
    private bool undo;
    public CancelableState( ITurnState wrappee, bool undo = true ) : base( wrappee )
    {
        this.undo = undo;
    }

    // Update is called once per frame
    public override void Update( TurnState context )
    {
        if ( Input.GetButtonDown( "Cancel" ) )
        {
            Input.ResetInputAxes();
            if ( undo == true )
                context.UndoEverything();
            context.GoToPreviousState();
        }
        base.Update( context );
	}
}
