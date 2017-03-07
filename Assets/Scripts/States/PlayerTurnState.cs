using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTurnState : TurnState
{
    public PlayerTurnState()
    {
        State = ChoosingUnitState.Instance;
        ControlledUnits = new HashSet<Unit>( 
            GameObject.FindGameObjectsWithTag( "Player" )
            .Select( obj => obj.GetComponent<Unit>() )
            .ToList() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );

        State.Enter( sys );
    }
}