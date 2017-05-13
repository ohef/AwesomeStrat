using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : TurnController
{
    private Vector2Int DoMove( Unit myUnit, Unit bestTarget )
    {
        GameMap                 map                = BattleSystem.Instance.Map;
        Vector2Int              bestTargetPosition = map.UnitPos[ bestTarget ];
        IEnumerable<Vector2Int> tilesAround        = 
            map.GetTilesWithinAbsoluteRange( bestTargetPosition,
            ( myUnit.Abilities.First( ability => ability is AttackAbility ) as AttackAbility ).Range )
            .Where( pos => map.Occupied( pos ) == false && pos != bestTargetPosition );

        IEnumerable<Vector2Int> pathToAttackPosition = null;

        foreach ( var viableAttack in tilesAround )
        {
            pathToAttackPosition = MapSearcher.Search( map.UnitPos[ myUnit ], viableAttack, map );
            if ( pathToAttackPosition != null )
                break;
        }

        if ( pathToAttackPosition == null )
            return map.UnitPos[ myUnit ];

        if ( pathToAttackPosition.Count() > myUnit.MovementRange )
        {
            pathToAttackPosition = pathToAttackPosition.Take( myUnit.MovementRange ).ToList();
        }

        BattleSystem.Instance.CreateMoveCommand( pathToAttackPosition, myUnit ).Execute();
        return pathToAttackPosition.Last();
    }

    private IEnumerable<Unit> GetEnemies()
    {
        return BattleSystem.Instance.Map.UnitPos
            .Where( kvp => ControlledUnits.Contains( kvp.Key ) == false )
            .Select( kvp => kvp.Key );
    }

    private Unit GetBestTarget( IEnumerable<Unit> enemyUnits )
    {
        return enemyUnits.OrderBy( unit => unit.HP ).First();
    }

    public override void EnterState()
    {
        base.EnterState();
        GameMap map = BattleSystem.Instance.Map;
        //Get ordering for Units
        foreach ( var myUnit in ControlledUnits )
        {
            var enemies = GetEnemies();
            if ( enemies.Count() == 0 )
            {
                BattleSystem.Instance.EndTurn();
                return;
            }

            Unit bestTarget = GetBestTarget( enemies );

            Vector2Int positionMoved = DoMove( myUnit, bestTarget );
            AttackAbility attackAbility = myUnit.Abilities.First( ability => ability is AttackAbility ) as AttackAbility;

            Unit targetInSight = map.GetUnitsWithinRange( positionMoved, attackAbility.Range )
                                    .Where( attackAbility.CanTargetFunction( this ) )
                                    .FirstOrDefault( unit => unit == bestTarget );

            if ( targetInSight != null )
                attackAbility.ExecuteOnTarget( bestTarget );
        }
        BattleSystem.Instance.EndTurn();
    }
}

//public interface IDecisionTreeNode
//{
//    IDecisionTreeNode MakeDecision();
//}

//public abstract class Decision<T> : IDecisionTreeNode
//{
//    public IDecisionTreeNode TrueNode;
//    public IDecisionTreeNode FalseNode;
//    public T TestValue;
//    public abstract bool GetBranch();
//    public IDecisionTreeNode MakeDecision()
//    {
//        if ( GetBranch() )
//            return TrueNode == null ? null : TrueNode.MakeDecision();
//        else
//            return FalseNode == null ? null : FalseNode.MakeDecision();
//    }
//}

//public class CanAttack : Decision<Unit>
//{
//    public override bool GetBranch()
//    {
//        //GameTile unitTile = BattleSystem.Instance.Map.UnitGametileMap[ TestValue ];
//        return true;
//    }
//}

//public abstract class ActionNode<T> : IDecisionTreeNode
//{
//    public T ActionValue; 
//    public IDecisionTreeNode MakeDecision()
//    {
//        return this;
//    }

//    public abstract void Execute();
//}

//public class DebugAction : ActionNode<object>
//{
//    public override void Execute()
//    {
//        Debug.Log( "Hey it's a debug action" );
//    }
//}