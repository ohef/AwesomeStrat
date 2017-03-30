using Assets.General;
using Assets.General.UnityExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchAbility : TargetAbility
{
    public SwitchAbility()
    {
        Targets = AbilityTargets.Friendly;
    }

    public override void ExecuteOnTarget( Unit target )
    {
        var sys = BattleSystem.Instance;
        var ownerTile = sys.Map.UnitGametileMap[ this.Owner ];
        var targetTile = sys.Map.UnitGametileMap[ target ];
        ChangePosition( this.Owner, targetTile );
        ChangePosition( target, ownerTile );
    }

    public void ChangePosition( Unit unit, GameTile to )
    {
        BattleSystem.Instance.Map.UnitGametileMap.Add( unit, to );
        unit.StartCoroutine(
            CustomAnimation.MotionTweenLinear
            ( unit.transform, to.Position.ToVector3(), 0.11f ) );
    }
}