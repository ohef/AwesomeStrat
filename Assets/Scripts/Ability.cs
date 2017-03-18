using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract partial class Ability
{
    public Unit Owner;
    public string Name;
    public Image AbliityImage;
}

public enum AbilityTargets
{
    All,
    Enemy,
    Friendly
}

public abstract partial class TargetAbility : Ability
{
    public AbilityTargets Targets = AbilityTargets.All;
    public int Range = 0;
}

public abstract partial class AreaOfEffectAbility : Ability
{
    public AbilityTargets Targets = AbilityTargets.All;
    public int Range = 0;
}

public interface IAbilityCreateState
{
    void CreateState( TargetAbility ability, TurnState context );
    void CreateState( AreaOfEffectAbility ability, TurnState context );
    void CreateState( WaitAbility ability, TurnState context );
}

public abstract partial class Ability
{
    public abstract void Accept( IAbilityCreateState visitor, TurnState context );
}

public abstract partial class TargetAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, TurnState context )
    {
        visitor.CreateState( this, context );
    }

    public abstract void ExecuteOnTarget( Unit target );
}

public abstract partial class AreaOfEffectAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, TurnState context )
    {
        visitor.CreateState( this, context );
    }
}

public class WaitAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, TurnState context )
    {
        visitor.CreateState( this, context );
    }
}

public class AttackAbility : TargetAbility
{
    public AttackAbility()
    {
        Targets = AbilityTargets.All;
    }

    public override void ExecuteOnTarget( Unit target )
    {
        target.HP -= Mathf.Min( Owner.Attack - target.Defense );
    }
}