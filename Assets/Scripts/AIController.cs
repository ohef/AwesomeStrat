using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class AIController : TurnController
{
    public AIController( int PlayerNumber, Color color ) : base( PlayerNumber, color )
    {
    }

    public class AIAbilityScorer : IAbilityGeneric<int>
    {
        public Vector2Int PositionForCheck;
        public AIController Context;

        public int Visit( WaitAbility ability )
        {
            return 1;
        }

        public int Visit( AreaOfEffectAbility ability )
        {
            return 0;
        }

        public int Visit( TargetAbility ability )
        {
            GameMap map = BattleSystem.Instance.Map;
            IEnumerable<Unit> interactableUnits =
                ability.GetInteractableUnits(
                    map.GetUnitsWithinRange( PositionForCheck, ability.Range ),
                    ability.GetTargetPredicate( Context ) );
            if ( interactableUnits.Count() == 0 )
                return 0;
            else return interactableUnits.Count();
        }
    }

    public override void Enter( BattleSystem state )
    {
        foreach ( var myUnit in ControlledUnits )
        {
            GameMap map = BattleSystem.Instance.Map;
            IEnumerable<Vector2Int> unitMovement = map.GetValidMovementPositions( myUnit );

            Vector2Int bestPosition = unitMovement.Select(
                delegate ( Vector2Int pos )
                {
                    AIAbilityScorer scorer = new AIAbilityScorer { PositionForCheck = pos, Context = this };
                    return new { Score = myUnit.Abilities.Select( ability => ability.Accept( scorer ) ).OrderByDescending( i => i ).First(), Position = pos };
                } ).OrderBy( item => item.Score ).First().Position;
            BattleSystem.Instance.CreateMoveCommand( new LinkedList<Vector2Int>( MapSearcher.Search( map.UnitPos[ myUnit ], bestPosition, map ) ), myUnit ).Execute();
        }
        BattleSystem.Instance.EndTurn();
    }

    public override void Exit( BattleSystem state )
    {
    }

    public override void Update( BattleSystem state )
    {
    }
}

public interface IDecisionTreeNode
{
    IDecisionTreeNode MakeDecision();
}

public abstract class Decision<T> : IDecisionTreeNode
{
    public IDecisionTreeNode TrueNode;
    public IDecisionTreeNode FalseNode;
    public T TestValue;
    public abstract bool GetBranch();
    public IDecisionTreeNode MakeDecision()
    {
        if ( GetBranch() )
            return TrueNode == null ? null : TrueNode.MakeDecision();
        else
            return FalseNode == null ? null : FalseNode.MakeDecision();
    }
}

public class CanAttack : Decision<Unit>
{
    public override bool GetBranch()
    {
        //GameTile unitTile = BattleSystem.Instance.Map.UnitGametileMap[ TestValue ];
        return true;
    }
}

public abstract class ActionNode<T> : IDecisionTreeNode
{
    public T ActionValue; 
    public IDecisionTreeNode MakeDecision()
    {
        return this;
    }

    public abstract void Execute();
}

public class DebugAction : ActionNode<object>
{
    public override void Execute()
    {
        Debug.Log( "Hey it's a debug action" );
    }
}