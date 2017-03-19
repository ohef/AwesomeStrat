using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class AreaOfEffectAbility : Ability
{
    public AbilityTargets Targets = AbilityTargets.All;
    public int Range = 0;
}