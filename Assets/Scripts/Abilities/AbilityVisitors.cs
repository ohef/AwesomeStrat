using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityCreateState
{
    void CreateState( TargetAbility ability, PlayerTurnController context );
    void CreateState( AreaOfEffectAbility ability, PlayerTurnController context );
    void CreateState( WaitAbility ability, PlayerTurnController context );
}

public abstract partial class Ability
{
    public abstract void Accept( IAbilityCreateState visitor, PlayerTurnController context );
}

public abstract partial class TargetAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, PlayerTurnController context )
    {
        visitor.CreateState( this, context );
    }
}

public abstract partial class AreaOfEffectAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, PlayerTurnController context )
    {
        visitor.CreateState( this, context );
    }
}