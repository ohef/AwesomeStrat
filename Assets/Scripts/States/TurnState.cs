using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnState : IPlayerState
{
    protected BattleSystem sys { get { return BattleSystem.Instance; } }

    public HashSet<Unit> ControlledUnits;
    public HashSet<Unit> HasNotActed;

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

    private Stack<IUndoCommand> Commands = new Stack<IUndoCommand>();

    public void GoToPreviousState()
    {
        if ( State == ChoosingUnitState.Instance )
            return;

        IPlayerState poppedState = StateStack.Pop();
        poppedState.Exit( sys );
        State.Enter( sys );
    }

    public void UndoEverything()
    {
        foreach ( var command in Commands )
            command.Undo();

        Commands.Clear();
    }

    public void GoToStateAndForget( BattleState state )
    {
        IPlayerState poppedState = StateStack.Pop();
        poppedState.Exit( sys );
        state.Enter( sys );
        StateStack.Clear();
        State = state;
    }

    public void DoCommand( IUndoCommand command )
    {
        Commands.Push( command );
        command.Execute();
    }

    public virtual void Enter( BattleSystem sys )
    {
        State = ChoosingUnitState.Instance;
        State.Enter( sys );
        HasNotActed = new HashSet<Unit>( ControlledUnits );
    }

    public virtual void Exit( BattleSystem sys )
    {
        State.Exit( sys );
        StateStack.Clear();
        Commands.Clear();
    }

    public virtual void Update( BattleSystem sys )
    {
        State.Update( sys );
    }
}
