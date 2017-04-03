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

            Vector2Int bestPosition = map.GetValidMovementPositions( myUnit, map.UnitGametileMap[ myUnit ] ).Select(
                delegate ( Vector2Int pos )
                {
                    AIAbilityScorer scorer = new AIAbilityScorer { PositionForCheck = pos, Context = this };
                    return new { Score = myUnit.Abilities.Select( ability => ability.Accept( scorer ) ).OrderByDescending( i => i ).First(), Position = pos };
                } ).OrderBy( item => item.Score ).First().Position;
            BattleSystem.Instance.CreateMoveCommand( new LinkedList<GameTile>( MapSearcher.Search( map.UnitGametileMap[ myUnit ], map[ bestPosition ], map ) ), myUnit ).Execute();
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