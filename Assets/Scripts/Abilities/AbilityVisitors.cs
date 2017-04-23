using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityVisitor<T>
{
    T Visit( TargetAbility ability);
    T Visit( AreaOfEffectAbility ability );
    T Visit( WaitAbility ability );
}

public abstract partial class Ability
{
    public abstract T Accept<T>( IAbilityVisitor<T> visitor );
}

public abstract partial class TargetAbility : Ability
{
    public override T Accept<T>( IAbilityVisitor<T> visitor )
    {
        return visitor.Visit( this );
    }
}

public abstract partial class AreaOfEffectAbility : Ability
{
    public override T Accept<T>( IAbilityVisitor<T> visitor )
    {
        return visitor.Visit( this );
    }
}

public partial class WaitAbility : Ability
{
    public override T Accept<T>( IAbilityVisitor<T> visitor )
    {
        return visitor.Visit( this );
    }
}