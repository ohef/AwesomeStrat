using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CancelableState : BattleState, ICancelHandler
{
    public bool undo;

    public void OnCancel( BaseEventData eventData )
    {
        Input.ResetInputAxes();
        if ( undo == true )
            sys.UndoEverything();
        sys.GoToPreviousState();
    }
}
