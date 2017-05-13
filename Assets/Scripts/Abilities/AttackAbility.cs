using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IUnitDamagedHandler : IEventSystemHandler
{
    void UnitDamaged( int damageDealt );
}

public class AttackAbility : TargetAbility
{
    public override void ExecuteOnTarget( Unit target )
    {
        int damageToDeal = Owner.AttackPower;

        target.HP -= damageToDeal;
        if ( damageToDeal > 0 )
        {
            ExecuteEvents.Execute<IUnitDamagedHandler>( target.gameObject, null,
                ( x, y ) => x.UnitDamaged( damageToDeal ) );
        }
    }
}