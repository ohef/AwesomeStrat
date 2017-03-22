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

    public PushAbility()
    {
        Targets = AbilityTargets.Friendly;
        Range = 1; 
    }

    public override Predicate<Unit> GetTargetPredicate( TurnState context )
    {
        return
        delegate ( Unit target )
        {
            GameTile finalPoint = GetComputedPushTile( target );
            return finalPoint == null ? false :
            BattleSystem.Instance.Map.Occupied( finalPoint ) == false 
            && base.GetTargetPredicate( context )( target );
        };
    }

    private GameTile GetComputedPushTile( Unit target )
    {
        BattleSystem sys = BattleSystem.Instance;
        Vector2Int direction = sys.Map.UnitGametileMap[ target ].Position - sys.Map.UnitGametileMap[ Owner ].Position;
        Vector2Int computedPoint = sys.Map.UnitGametileMap[ target ].Position + direction;

        if ( sys.Map.IsOutOfBounds( computedPoint ) == false )
            return sys.Map[ computedPoint ];
        else
            return null;
    }

    public override void ExecuteOnTarget( Unit target )
    {
        GameTile finalPoint = GetComputedPushTile( target );

        BattleSystem.Instance.Map.PlaceUnit( target, finalPoint );
        target.StartCoroutine( CustomAnimation.MotionTweenLinear( target.transform, finalPoint.Position.ToVector3(), 0.11f ) );
    }
}