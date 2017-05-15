using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuState : BattleState
{
    public override void Exit()
    {
        base.Exit();
        sys.Menu.ClearButtons();
        EventSystem.current.SetSelectedGameObject( sys.gameObject );
    }
}

