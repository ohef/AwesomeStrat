using Assets.General.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BattleState : MonoBehaviour, IState
{
    protected BattleSystem sys { get { return BattleSystem.Instance; } }

    public void Transition<T>( Action<T> initializer ) where T : BattleState
    {
        var state = sys.GetState<T>();
        initializer( state );
        sys.State = state;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }
}