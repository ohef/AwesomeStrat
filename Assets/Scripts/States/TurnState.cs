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
    private Stack<IUndoCommand> CommandsForReEnter = new Stack<IUndoCommand>();

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
        ClearManagementHistory();
        state.Enter( sys );
        State = state;
    }

    public void ClearManagementHistory()
    {
        StateStack.Clear();
        Commands.Clear();
    }

    public void DoCommand( IUndoCommand command )
    {
        Commands.Push( command );
        command.Execute();
    }

    public void UnitFinished( Unit unit )
    {
        foreach ( var ren in unit.GetComponentsInChildren<SkinnedMeshRenderer>() )
        {
            UndoCommandAction setFinishedColor = new UndoCommandAction (
                delegate { ren.material.color = new Color( 0.75f, 0.75f, 0.75f ); },
                delegate { ren.material.color = new Color( 1.00f, 1.00f, 1.00f ); } );
            setFinishedColor.Execute();
            CommandsForReEnter.Push( setFinishedColor );
        }
        HasNotActed.Remove( unit );
    }

    public virtual void Enter( BattleSystem sys )
    {
        State = ChoosingUnitState.Instance;
        State.Enter( sys );
        RefreshTurn();
        HasNotActed = new HashSet<Unit>( ControlledUnits );
    }

    private void RefreshTurn()
    {
        foreach ( var c in CommandsForReEnter )
            c.Undo();
        CommandsForReEnter.Clear();
    }

    public virtual void Exit( BattleSystem sys )
    {
        State.Exit( sys );
        ClearManagementHistory();
    }

    public virtual void Update( BattleSystem sys )
    {
        State.Update( sys );
    }
}
