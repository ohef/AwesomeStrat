using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CancelButton : Button, ICancelHandler
{
    public void OnCancel( BaseEventData eventData )
    {
        ExecuteEvents.Execute( BattleSystem.Instance.gameObject, eventData, ExecuteEvents.cancelHandler );
    }
}
