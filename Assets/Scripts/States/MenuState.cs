using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuState : CancelableState
{
    public override void Exit( BattleSystem sys )
    {
        //To prevent the next state from catching the submit button
        Input.ResetInputAxes();
        sys.Menu.ClearButtons();
    }
}

