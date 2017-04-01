using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCursorState : DecoratorState
{
    public ControlCursorState( ITurnState wrappee ) : base( wrappee ) { }

    public override void Update( PlayerTurnController context )
    {
        sys.Cursor.UpdateAction();
        base.Update( context );
    }
}