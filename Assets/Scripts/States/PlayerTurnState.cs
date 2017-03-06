using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnState : TurnState
{
    public PlayerTurnState()
    {
        State = ChoosingUnitState.Instance;
        State.Enter( sys );
    }
}