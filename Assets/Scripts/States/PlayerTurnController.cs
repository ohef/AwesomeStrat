using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapActionsController
{
    private Stack<IUndoCommand> History = new Stack<IUndoCommand>();

    private delegate void ExecuteDelegate( IUndoCommand command );
    private ExecuteDelegate CurrentTransaction;

    public void ExecuteCommand( IUndoCommand command )
    {
        History.Push( command );
        command.Execute();
    }
}

public interface IUnitMemento
{
    UnitMemento CreateMemento();
    void SetMemento( UnitMemento memento );
    void SetPositionFromMemento( UnitMemento memento );
    void SetMaterialBlockFromMemento( UnitMemento memento );
}

public class UnitMemento
{
    public MaterialPropertyBlock MaterialProperty { get; private set; }
    public Vector3 Position { get; private set; }

    public UnitMemento( Unit unit )
    {
        MaterialProperty = new MaterialPropertyBlock();
        unit.transform
            .Find( "Portrait" )
            .GetComponent<Renderer>()
            .GetPropertyBlock( MaterialProperty );
        Position = unit.GetComponent<Transform>().position;
    }
}

//public class PlayerTurnController : TurnController 
//{
//    protected BattleSystem sys { get { return BattleSystem.Instance; } }

//    private Stack<BattleState> StateStack = new Stack<BattleState>();
//    public BattleState State
//    {
//        get { return StateStack.Peek(); }
//        set
//        {
//            if ( StateStack.Count > 0 )
//                TransitionCalls( StateStack.Peek(), value );
//            else
//                TransitionCalls( value, value );

//            StateStack.Push( value );
//        }
//    }

//    private Stack<IUndoCommand> Commands = new Stack<IUndoCommand>();
//    private Stack<IUndoCommand> CommandsForReEnter = new Stack<IUndoCommand>();

//    public void GoToPreviousState()
//    {
//        if ( State is ChoosingUnitState )
//            return;

//        TransitionCalls( StateStack.Pop(), State );
//    }

//    private void TransitionCalls( BattleState from, BattleState to )
//    {
//        from.Exit();
//        to.Enter();
//    }

//    public void UndoEverything()
//    {
//        foreach ( var command in Commands )
//            command.Undo();

//        Commands.Clear();
//    }

//    public void GoToStateAndForget( BattleState state )
//    {
//        TransitionCalls( StateStack.Pop(), state );
//        ClearStateHistory();
//        State = state;
//    }

//    public void ClearStateHistory()
//    {
//        StateStack.Clear();
//        Commands.Clear();
//    }

//    public void DoCommand( IUndoCommand command )
//    {
//        Commands.Push( command );
//        command.Execute();
//    }

//    public void UnitFinished( Unit unit )
//    {
//        var ren = unit.GetComponentInChildren<SpriteRenderer>();
//        var propBlock = new MaterialPropertyBlock();
//        ren.GetPropertyBlock( propBlock );

//        UndoCommandAction setFinishedColor = new UndoCommandAction(
//                delegate { propBlock.SetColor( "_Color", new Color( 0.75f, 0.75f, 0.75f ) ); ren.SetPropertyBlock( propBlock ); },
//                delegate { propBlock.SetColor( "_Color", new Color( 1.00f, 1.00f, 1.00f ) ); ren.SetPropertyBlock( propBlock ); }
//                );

//        setFinishedColor.Execute();
//        CommandsForReEnter.Push( setFinishedColor );
//        HasNotActed.Remove( unit );
//    }

//    private void RefreshTurn()
//    {
//        foreach ( var c in CommandsForReEnter )
//            c.Undo();
//        CommandsForReEnter.Clear();
//    }

//    public void Start()
//    {
//        HasNotActed = new HashSet<Unit>( ControlledUnits );
//    }

//    //public void EnterState()
//    //{
//    //    State = sys.GetState<ChoosingUnitState>();
//    //}

//    //public override void ExitState()
//    //{
//    //    State.Exit();
//    //    RefreshTurn();
//    //    ClearStateHistory();
//    //}
//}