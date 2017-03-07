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
        ControlledUnits = new HashSet<Unit>( 
            sys.UnitLayer.GetComponentsInChildren<Unit>()
            .Where( unitPredicate )
            .Select( obj => obj.GetComponent<Unit>() )
            .ToList() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );

        State.Enter( sys );
    }
}