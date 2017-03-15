using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTurnState : TurnState
{
    public PlayerTurnState( Func<Unit, bool> unitPredicate )
    {
        State = ChoosingUnitState.Instance;
        var controlledUnits = sys.UnitLayer.GetComponentsInChildren<Unit>()
           .Where( unitPredicate ).ToList();

        foreach ( var unit in controlledUnits )
        {
            foreach ( Transform obj in unit.transform )
            {
                if ( obj.name == "ControlIndicator" )
                {
                    obj.GetComponent<MeshRenderer>().material.color = new Color( 0.25f, 0.25f, 0.75f );
                }
            }
        }

        ControlledUnits = new HashSet<Unit>( controlledUnits
            .Select( obj => obj.GetComponent<Unit>() )
            .ToList() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );

        State.Enter( this );
    }
}