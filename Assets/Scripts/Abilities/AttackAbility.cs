using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IUnitEventsHandler : IEventSystemHandler
{
    void UnitDamaged( int damageDealt );
}

public class AttackAbility : TargetAbility
{
    public int AttackPower;

    public override void ExecuteOnTarget( Unit target )
    {
        this.Owner.Animator.SetTrigger( "Attack" );

        int damageDone = Mathf.Min( AttackPower - target.Defense );
        target.HP -= damageDone;
        if ( damageDone > 0 )
        {
            ExecuteEvents.Execute<IUnitEventsHandler>( target.gameObject, null,
                ( x, y ) => x.UnitDamaged( damageDone ) );
        }
    }
}