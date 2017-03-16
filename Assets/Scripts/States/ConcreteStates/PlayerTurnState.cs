using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTurnState : TurnState
{
    public PlayerTurnState( Func<Unit, bool> unitPredicate, Color color )
    {
        State = ChoosingUnitState.Instance;
        var controlledUnits = sys.UnitLayer.GetComponentsInChildren<Unit>()
           .Where( unitPredicate ).ToList();

        foreach ( var unit in controlledUnits )
        {
            unit.GetComponent<UnitGraphics>()
                .UnitIndicator.material.color =
                color;
        }

        ControlledUnits = new HashSet<Unit>( controlledUnits
            .Select( obj => obj.GetComponent<Unit>() )
            .ToList() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );

        State.Enter( this );
    }
}