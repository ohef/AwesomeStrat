using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DecoratorState : BattleState, ITurnState
{
    private ITurnState wrappee;

    public DecoratorState( ITurnState wrappee )
    {
        this.wrappee = wrappee;
    }

    public override void Update( TurnState context )
    {
        wrappee.Update( context );
    }

    public override void Enter( TurnState context )
    {
        wrappee.Enter( context );
    }

    public override void Exit( TurnState context )
    {
        wrappee.Exit( context );
    }
}
