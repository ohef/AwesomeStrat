using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuState : BattleState
{
    public override void Enter()
    {
        var child = sys.Menu.transform.GetChild( 0 );
        if ( child != null )
            EventSystem.current.SetSelectedGameObject( child.gameObject );
    }

    public override void Exit()
    {
        sys.Menu.ClearButtons();
        EventSystem.current.SetSelectedGameObject( sys.gameObject );
    }
}