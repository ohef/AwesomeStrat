using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleState : MonoBehaviour
{
    protected BattleSystem sys { get { return BattleSystem.Instance; } }
    private PlayerTurnController context;

    protected PlayerTurnController Context
    {
        get
        {
            if ( context == null )
                context = sys.CurrentTurn as PlayerTurnController;
            return context;
        }
    }
}