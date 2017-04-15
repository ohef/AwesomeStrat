using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitEventData : BaseEventData
{
    public Unit Unit;
    public UnitEventData( EventSystem eventSystem, Unit unit ) : base( eventSystem )
    {
        Unit = unit;
    }
}

public interface IUnitEventsHandler : IEventSystemHandler
{
    void UnitDamaged( UnitEventData data, int damageDealt );
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
            ExecuteEvents.Execute<IUnitEventsHandler>( target.gameObject, new UnitEventData( EventSystem.current, target ),
                ( x, y ) => x.UnitDamaged( ExecuteEvents.ValidateEventData<UnitEventData>( y ), damageDone ) );
        }
    }
}