using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAbility : TargetAbility
{
    public AttackAbility()
    {
        Targets = AbilityTargets.Enemy;
    }

    public override void ExecuteOnTarget( Unit target )
    {
        target.HP -= Mathf.Min( Owner.Attack - target.Defense );
    }
}