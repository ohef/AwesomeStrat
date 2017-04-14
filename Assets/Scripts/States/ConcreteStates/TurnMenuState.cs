using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnMenuState : MenuState
{
    public void OnEnable()
    {
        var def = sys.Menu.AddButton( "End Turn", sys.EndTurn );
        EventSystem.current.SetSelectedGameObject( def.gameObject );
    }
}
