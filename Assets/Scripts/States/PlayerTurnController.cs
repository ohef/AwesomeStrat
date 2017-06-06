using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IMonoBehaviourState
{
    void EnterState();
    void ExitState();
    void UpdateState();
}

public class PlayerTurnController : TurnController, ISubmitHandler, IMoveHandler, ICancelHandler
{
    protected BattleSystem sys { get { return BattleSystem.Instance; } }

    private Stack<BattleState> StateStack = new Stack<BattleState>();
    public BattleState State
    {
        get { return StateStack.Peek(); }
        set
        {
            if ( StateStack.Count > 0 )
                TransitionCalls( StateStack.Peek(), value );
            else
                TransitionCalls( value, value );

            StateStack.Push( value );
        }
    }

    private Stack<IUndoCommand> Commands = new Stack<IUndoCommand>();
    private Stack<IUndoCommand> CommandsForReEnter = new Stack<IUndoCommand>();

    public void GoToPreviousState()
    {
        if ( State is ChoosingUnitState )
            return;

        TransitionCalls( StateStack.Pop(), State );
    }

    private void TransitionCalls( BattleState from, BattleState to )
    {
        from.Exit();
        to.Enter();
    }

    public void UndoEverything()
    {
        foreach ( var command in Commands )
            command.Undo();

        Commands.Clear();
    }

    public void GoToStateAndForget( BattleState state )
    {
        TransitionCalls( StateStack.Pop(), state );
        ClearStateHistory();
        State = state;
    }

    public void ClearStateHistory()
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
        var ren = unit.GetComponentInChildren<SpriteRenderer>();
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
        base.EnterState();
        State = sys.GetState<ChoosingUnitState>();
        HasNotActed = new HashSet<Unit>( ControlledUnits );
    }

    public override void ExitState()
    {
        base.ExitState();
        State.Exit();
        RefreshTurn();
        ClearStateHistory();
    }

    public void OnSubmit( BaseEventData eventData )
    {
        ExecuteEvents.Execute<ISubmitHandler>( State.gameObject, eventData, ExecuteEvents.submitHandler );
    }

    public void OnMove( AxisEventData eventData )
    {
        ExecuteEvents.Execute<IMoveHandler>( State.gameObject, eventData, ExecuteEvents.moveHandler );
    }

    public void OnCancel( BaseEventData eventData )
    {
        ExecuteEvents.Execute<ICancelHandler>( State.gameObject, eventData, ExecuteEvents.cancelHandler );
    }
}