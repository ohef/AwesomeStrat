using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AreaOfEffectAbility : Ability
{
    public AbilityTargets Targets = AbilityTargets.All;
    public int Range = 0;

    public override T Accept<T>( IAbilityVisitor<T> visitor )
    {
        return visitor.Visit( this );
    }
}