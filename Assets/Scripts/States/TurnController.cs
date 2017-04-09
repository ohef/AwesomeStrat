using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class TurnController : ISystemState
{
    public HashSet<Unit> ControlledUnits;
    public HashSet<Unit> HasNotActed;
    public int PlayerNo;

    public bool DoesControl( UnitMapHelper unit, int PlayerNumber )
    {
        return unit.PlayerOwner == PlayerNumber;
    }

    public TurnController( int PlayerNumber, Color color )
    {
        PlayerNo = PlayerNumber;
        var controlledUnits = BattleSystem.Instance.Map.UnitPos
            .Select<Unit, UnitMapHelper>( unit => unit.GetComponent<UnitMapHelper>() )
            .Where( unit => DoesControl( unit, PlayerNumber ) ).ToList();

        foreach ( var unit in controlledUnits )
        {
            unit.GetComponent<UnitGraphics>()
                .UnitIndicator.material.color = color;
        }

        ControlledUnits = new HashSet<Unit>( controlledUnits
            .Select( obj => obj.GetComponent<Unit>() )
            .ToList() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );
    }


    public abstract void Enter( BattleSystem state );
    public abstract void Exit( BattleSystem state );
    public abstract void Update( BattleSystem state );
}
