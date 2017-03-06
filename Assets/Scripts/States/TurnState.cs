using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnState : IPlayerState
{
    protected BattleSystem sys { get { return BattleSystem.Instance; } }

    private Stack<BattleState> StateStack = new Stack<BattleState>();
    public BattleState State
    {
        get { return StateStack.Peek(); }
        set
        {
            if ( StateStack.Count > 0 )
            {
                StateStack.Peek().Exit( sys );
                value.Enter( sys );
            }
            StateStack.Push( value );
        }
    }

    public Stack<IUndoCommand> Commands = new Stack<IUndoCommand>();

    public void GoToPreviousState()
    {
        if ( State == ChoosingUnitState.Instance )
            return;

        IPlayerState poppedState = StateStack.Pop();
        poppedState.Exit( sys );
        State.Enter( sys );
    }

    public void GoToStateAndForget( BattleState state )
    {
        IPlayerState poppedState = StateStack.Pop();
        poppedState.Exit( sys );
        state.Enter( sys );
        StateStack.Clear();
        State = state;
    }

    public virtual void Enter( BattleSystem sys ) { }

    public virtual void Exit( BattleSystem sys ) { }

    public virtual void Update( BattleSystem sys )
    {
        State.Update( sys );
    }
}
