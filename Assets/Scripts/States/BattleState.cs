using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleState : IPlayerState
{
    protected BattleSystem sys { get { return BattleSystem.Instance; } }

    public virtual void Update( BattleSystem sys ) { }

    public virtual void Enter( BattleSystem sys ) { }

    public virtual void Exit( BattleSystem sys ) { }
}