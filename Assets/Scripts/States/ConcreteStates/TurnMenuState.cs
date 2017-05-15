using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TurnMenuState : MenuState
{
    public override void Enter()
    {
        Button button = sys.Menu.AddButton( "End Turn", sys.EndTurn );
        EventSystem.current.SetSelectedGameObject( button.gameObject );
    }
}
