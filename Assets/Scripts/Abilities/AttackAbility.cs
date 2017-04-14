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

public interface IUnitEventHandler : IEventSystemHandler
{
    void UnitDamaged( UnitEventData data, int preDamage );
}

public class AttackAbility : TargetAbility
{
    public int AttackPower;

    public override void ExecuteOnTarget( Unit target )
    {
        int preDamage = target.HP;

        target.HP -= Mathf.Min( AttackPower - target.Defense );

        ExecuteEvents.Execute<IUnitEventHandler>(
            target.gameObject,
            new UnitEventData( EventSystem.current, target ),
            ( x, y ) => x.UnitDamaged( ExecuteEvents.ValidateEventData<UnitEventData>( y ), preDamage ) );
    }
}