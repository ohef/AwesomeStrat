using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class TargetAbility : Ability
{
    public AbilityTargets Targets = AbilityTargets.All;
    public int Range = 1;

    public abstract void ExecuteOnTarget( Unit target );

    public IEnumerable<Unit> GetInteractableUnits(
    IEnumerable<Unit> Units, Predicate<Unit> UseableOn )
    {
        foreach ( var unitToCheck in Units )
            if ( UseableOn( unitToCheck ) )
                yield return unitToCheck;
            else
                continue;
        yield break;
    }

    public virtual Predicate<Unit> GetTargetPredicate( TurnController context )
    {
        Predicate<Unit> predicate = unit => true;
        switch ( this.Targets )
        {
            case AbilityTargets.Enemy:
                predicate = unit => !context.ControlledUnits.Contains( unit ) && unit != this.Owner;
                break;
            case AbilityTargets.Friendly:
                predicate = unit => context.ControlledUnits.Contains( unit ) && unit != this.Owner;
                break;
        }
        return predicate;
    }
}