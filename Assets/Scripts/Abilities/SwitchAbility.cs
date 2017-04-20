using Assets.General;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;

public class SwitchAbility : TargetAbility
{
    public override void ExecuteOnTarget( Unit target )
    {
        var sys = BattleSystem.Instance;
        var ownerTile = sys.Map.UnitPos[ this.Owner ];
        var targetTile = sys.Map.UnitPos[ target ];
        ChangePosition( this.Owner, targetTile );
        ChangePosition( target, ownerTile );
    }

    public void ChangePosition( Unit unit, Vector2Int to )
    {
        BattleSystem.Instance.Map.UnitPos.Add( unit, to );
        unit.StartCoroutine(
            CustomAnimation.MotionTweenLinear
            ( unit.transform, MapPositionTranslator.GetTransform( to ), 0.11f ) );
    }
}