using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IState
{
    void Enter();
    void Exit();
}

public abstract class BattleState : MonoBehaviour, IState
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

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }
}