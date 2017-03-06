using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlCursorState : CancelableState
{
    public override void Update( BattleSystem sys )
    {
        base.Update( sys );
        sys.Cursor.UpdateAction();
    }
}