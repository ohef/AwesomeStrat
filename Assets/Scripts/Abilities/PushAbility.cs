using Assets.General;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PushAbility : TargetAbility
{
    public int PushRange = 1;

    public override Func<Unit,bool> CanTargetFunction( TurnController context )
    {
        return
        delegate ( Unit target )
        {
            Vector2Int computedPoint = GetComputedPushPosition( target );
            bool occupied = BattleSystem.Instance.Map.Occupied( computedPoint );
            bool outOfBounds = BattleSystem.Instance.Map.IsOutOfBounds( computedPoint );

            return
            occupied == false &&
            outOfBounds == false &&
            base.CanTargetFunction( context )( target );
        };
    }

    private Vector2Int GetComputedPushPosition( Unit target )
    {
        BattleSystem sys = BattleSystem.Instance;
        Vector2Int direction = sys.Map.UnitPos[ target ] - sys.Map.UnitPos[ Owner ];
        return sys.Map.UnitPos[ target ] + direction;
    }

    public override void ExecuteOnTarget( Unit target )
    {
        Vector2Int finalPoint = GetComputedPushPosition( target );

        BattleSystem.Instance.Map.PlaceUnit( target, finalPoint );
        target.StartCoroutine( CustomAnimation.MotionTweenLinear( target.transform, MapPositionTranslator.GetTransform( finalPoint ), 0.11f ) );
    }
}