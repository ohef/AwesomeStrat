using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCursorState : BattleState
{
    public void Update()
    {
        sys.Cursor.UpdateAction();
    }
}