using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelableState : DecoratorState
{
    private bool undo;
    public CancelableState( IPlayerState wrappee, bool undo = true ) : base( wrappee )
    {
        this.undo = undo;
    }

    // Update is called once per frame
    public override void Update( BattleSystem sys )
    {
        if ( Input.GetButtonDown( "Cancel" ) )
        {
            Input.ResetInputAxes();
            if ( undo == true )
                sys.CurrentTurn.UndoEverything();
            sys.CurrentTurn.GoToPreviousState();
        }
        base.Update( sys );
	}
}
