using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WaitAbility : Ability
{
    public static WaitAbility Instance = new WaitAbility();
    public WaitAbility()
    {
        Name = "Wait";
    }
}