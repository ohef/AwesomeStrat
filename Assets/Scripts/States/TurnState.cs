using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnState : ISystemState
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
                StateStack.Peek().Exit( this );
                value.Enter( this );
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

        ITurnState poppedState = StateStack.Pop();
        poppedState.Exit( this );
        State.Enter( this );
    }

    public void UndoEverything()
    {
        foreach ( var command in Commands )
            command.Undo();

        Commands.Clear();
    }

    public void GoToStateAndForget( BattleState state )
    {
        ITurnState poppedState = StateStack.Pop();
        poppedState.Exit( this );
        ClearManagementHistory();
        state.Enter( this );
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

    private void RefreshTurn()
    {
        foreach ( var c in CommandsForReEnter )
            c.Undo();
        CommandsForReEnter.Clear();
    }

    public virtual void Enter( BattleSystem sys )
    {
        State = ChoosingUnitState.Instance;
        State.Enter( this );
        RefreshTurn();
        HasNotActed = new HashSet<Unit>( ControlledUnits );
    }

    public virtual void Exit( BattleSystem sys )
    {
        State.Exit( this );
        ClearManagementHistory();
    }

    public virtual void Update( BattleSystem sys )
    {
        State.Update( this );
    }
}
