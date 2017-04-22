using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public interface IMonoBehaviourState
{
    void EnterState();
    void ExitState();
    void UpdateState();
}

public class PlayerTurnController : TurnController
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
                StateStack.Peek().gameObject.SetActive( false );
                value.gameObject.SetActive( true );
            }
            StateStack.Push( value );
        }
    }

    private Stack<IUndoCommand> Commands = new Stack<IUndoCommand>();
    private Stack<IUndoCommand> CommandsForReEnter = new Stack<IUndoCommand>();

    public void GoToPreviousState()
    {
        if ( State is ChoosingUnitState )
            return;

        BattleState poppedState = StateStack.Pop();
        poppedState.gameObject.SetActive( false );
        State.gameObject.SetActive( true );
    }

    public void UndoEverything()
    {
        foreach ( var command in Commands )
            command.Undo();

        Commands.Clear();
    }

    public void GoToStateAndForget( BattleState state )
    {
        BattleState poppedState = StateStack.Pop();
        poppedState.gameObject.SetActive( false );
        ClearManagementHistory();
        state.gameObject.SetActive( true );
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
        var ren = unit.GetComponent<Renderer>();

        var propBlock = new MaterialPropertyBlock();
        ren.GetPropertyBlock( propBlock );

        UndoCommandAction setFinishedColor = new UndoCommandAction(
                delegate { propBlock.SetColor( "_Color", new Color( 0.75f, 0.75f, 0.75f ) ); ren.SetPropertyBlock( propBlock ); },
                delegate { propBlock.SetColor( "_Color", new Color( 1.00f, 1.00f, 1.00f ) ); ren.SetPropertyBlock( propBlock ); }
                );

        setFinishedColor.Execute();
        CommandsForReEnter.Push( setFinishedColor );
        HasNotActed.Remove( unit );
    }

    private void RefreshTurn()
    {
        foreach ( var c in CommandsForReEnter )
            c.Undo();
        CommandsForReEnter.Clear();
    }

    public void Start()
    {
        EnterState();
    }

    public override void EnterState()
    {
        State = sys.GetState<ChoosingUnitState>();
        State.gameObject.SetActive( true );
        HasNotActed = new HashSet<Unit>( ControlledUnits );
    }

    public override void ExitState()
    {
        State.gameObject.SetActive( false );
        RefreshTurn();
        ClearManagementHistory();
    }

    public override void UpdateState() { }
}
