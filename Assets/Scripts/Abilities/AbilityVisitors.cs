using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityVisitor<T>
{
    T Visit( TargetAbility ability);
    T Visit( AreaOfEffectAbility ability );
    T Visit( WaitAbility ability );
}