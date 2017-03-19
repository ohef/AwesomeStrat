using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class TargetAbility : Ability
{
    public AbilityTargets Targets = AbilityTargets.All;
    public int Range = 0;

    public abstract void ExecuteOnTarget( Unit target );
}