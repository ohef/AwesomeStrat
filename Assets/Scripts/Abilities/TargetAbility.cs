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
        Predicate<Unit> UseableOn, GameMap map )
    {
        foreach ( var tile in map.GetTilesWithinAbsoluteRange(
            map.UnitGametileMap[ this.Owner ].Position, this.Range ) )
        {
            Unit unitToCheck = null;
            if ( map.UnitGametileMap.TryGetValue( map[ tile ], out unitToCheck ) )
            {
                if ( UseableOn( unitToCheck ) )
                    yield return unitToCheck;
                else
                    continue;
            }
        }
        yield break;
    }

    public virtual Predicate<Unit> GetTargetPredicate( PlayerTurnController context )
    {
        Predicate<Unit> predicate = unit => true;
        if ( this.Targets == AbilityTargets.Enemy )
        {
            predicate = unit => !context.ControlledUnits.Contains( unit ) && unit != this.Owner;
        }
        else if ( this.Targets == AbilityTargets.Friendly )
        {
            predicate = unit => context.ControlledUnits.Contains( unit ) && unit != this.Owner;
        }

        return predicate;
    }
}