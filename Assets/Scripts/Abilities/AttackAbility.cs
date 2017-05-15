using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IUnitDamagedHandler : IEventSystemHandler
{
    void UnitDamaged( int damageDealt );
}

[CreateAssetMenu( menuName = "Ability/Attack" )]
public class AttackAbility : TargetAbility
{
    public int AttackDamage;
    public override void ExecuteOnTarget( Unit user, Unit target )
    {
        int damageToDeal = AttackDamage;

        target.HP -= damageToDeal;
        if ( damageToDeal > 0 )
        {
            ExecuteEvents.Execute<IUnitDamagedHandler>( target.gameObject, null,
                ( x, y ) => x.UnitDamaged( damageToDeal ) );
        }
    }
}