using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCursorState : DecoratorState
{
    public ControlCursorState( IPlayerState wrappee ) : base( wrappee )
    {
    }

    public override void Update( BattleSystem sys )
    {
        sys.Cursor.UpdateAction();
        base.Update( sys );
    }
}