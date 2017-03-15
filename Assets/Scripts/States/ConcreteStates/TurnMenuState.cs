using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnMenuState : MenuState
{
    public static BattleState Create()
    {
        return new CancelableState( new TurnMenuState() );
    }

    public override void Enter( TurnState context )
    {
        var def = sys.Menu.AddButton( "End Turn", sys.EndTurn );
        EventSystem.current.SetSelectedGameObject( def.gameObject );
    }
}
