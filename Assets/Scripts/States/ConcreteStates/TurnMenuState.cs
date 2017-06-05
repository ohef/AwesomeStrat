using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

sealed class TurnMenuState : MenuState
{
    public override void Enter()
    {
        Button button = sys.Menu.AddButton( "End Turn", sys.EndTurn );
        base.Enter();
    }
}
