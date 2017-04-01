using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuState : BattleState
{
    public override void Exit( PlayerTurnController context )
    {
        //To prevent the next state from catching the submit button
        Input.ResetInputAxes();
        sys.Menu.ClearButtons();
    }
}

