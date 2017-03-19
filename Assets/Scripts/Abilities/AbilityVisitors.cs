using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityCreateState
{
    void CreateState( TargetAbility ability, TurnState context );
    void CreateState( AreaOfEffectAbility ability, TurnState context );
    void CreateState( WaitAbility ability, TurnState context );
}

public abstract partial class Ability
{
    public abstract void Accept( IAbilityCreateState visitor, TurnState context );
}

public abstract partial class TargetAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, TurnState context )
    {
        visitor.CreateState( this, context );
    }
}

public abstract partial class AreaOfEffectAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, TurnState context )
    {
        visitor.CreateState( this, context );
    }
}