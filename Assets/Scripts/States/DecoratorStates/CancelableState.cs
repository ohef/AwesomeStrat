using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CancelableState : BattleState, ICancelHandler
{
    public void OnCancel( BaseEventData eventData )
    {
        Input.ResetInputAxes();
        sys.GoToPreviousState();
    }
}

public class UndoCancelableState : BattleState, ICancelHandler
{
    public void OnCancel( BaseEventData eventData )
    {
        Input.ResetInputAxes();
        sys.UndoEverything();
        sys.GoToPreviousState();
    }
}
