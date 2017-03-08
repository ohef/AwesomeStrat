using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DecoratorState : BattleState, IPlayerState
{
    private IPlayerState wrappee;

    public DecoratorState( IPlayerState wrappee )
    {
        this.wrappee = wrappee;
    }

    public override void Update( BattleSystem sys )
    {
        wrappee.Update( sys );
    }

    public override void Enter( BattleSystem sys )
    {
        wrappee.Enter( sys );
    }

    public override void Exit( BattleSystem sys )
    {
        wrappee.Exit( sys );
    }
}
