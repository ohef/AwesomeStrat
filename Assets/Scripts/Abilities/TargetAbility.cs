using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class TargetAbility : Ability
{
    public AbilityTargets Targets = AbilityTargets.All;
    public int Range = 1;

    public abstract void ExecuteOnTarget( Unit user, Unit target );

    public virtual Func<Unit, bool> CanTargetFunction( Unit user, TurnController context )
    {
        Func<Unit, bool> predicate = unit => true;
        switch ( this.Targets )
        {
            case AbilityTargets.Enemy:
                predicate = unit => !context.ControlledUnits.Contains( unit ) && unit != user;
                break;
            case AbilityTargets.Friendly:
                predicate = unit => context.ControlledUnits.Contains( unit ) && unit != user;
                break;
        }
        return predicate;
    }

    public override T Accept<T>( IAbilityVisitor<T> visitor )
    {
        return visitor.Visit( this );
    }
}