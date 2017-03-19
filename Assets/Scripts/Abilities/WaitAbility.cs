using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitAbility : Ability
{
    public override void Accept( IAbilityCreateState visitor, TurnState context )
    {
        visitor.CreateState( this, context );
    }
}
