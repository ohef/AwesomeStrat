using Assets.General;
using Assets.General.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushAbility : TargetAbility
{
    public PushAbility()
    {
        Targets = AbilityTargets.Friendly;
        Range = 1; 
    }

    public override void ExecuteOnTarget( Unit target )
    {
        var sys = BattleSystem.Instance;
        var direction = sys.Map.UnitGametileMap[ target ].Position - sys.Map.UnitGametileMap[ Owner ].Position;
        var finalPoint = sys.Map[ sys.Map.UnitGametileMap[ target ].Position + direction ];

        sys.Map.PlaceUnit( target, finalPoint );
        target.StartCoroutine( CustomAnimation.MotionTweenLinear( target.transform, finalPoint.Position.ToVector3(), 0.11f ) );
    }
}