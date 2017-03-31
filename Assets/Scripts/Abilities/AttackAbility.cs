using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAbility : TargetAbility
{
    public int AttackPower;

    public override void ExecuteOnTarget( Unit target )
    {
        target.HP -= Mathf.Min( AttackPower - target.Defense );
    }
}