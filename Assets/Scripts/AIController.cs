using System;
using UnityEngine;

internal class AIController : TurnController
{
    public AIController( int PlayerNumber, Color color ) : base( PlayerNumber, color )
    {
    }

    public override void Enter( BattleSystem state )
    {
        BattleSystem.Instance.EndTurn();
    }

    public override void Exit( BattleSystem state )
    {
    }

    public override void Update( BattleSystem state )
    {
    }
}