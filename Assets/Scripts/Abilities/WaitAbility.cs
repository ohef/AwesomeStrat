using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitAbility : Ability
{
    public WaitAbility()
    {
        Name = "Wait";
    }

    public override void Accept( IAbilityCreateState visitor, PlayerTurnController context )
    {
        visitor.CreateState( this, context );
    }
}
