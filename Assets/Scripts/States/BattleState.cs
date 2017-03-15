using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleState : ITurnState
{
    protected BattleSystem sys { get { return BattleSystem.Instance; } }

    public virtual void Update( TurnState context ) { }

    public virtual void Enter( TurnState context ) { }

    public virtual void Exit( TurnState context ) { }
}