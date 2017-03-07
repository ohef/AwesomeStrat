using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Assets.General.DataStructures;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Assets.General.UnityExtensions;

public class BattleSystem : MonoBehaviour
{
    private static BattleSystem instance;
    public static BattleSystem Instance { get { return instance; } }

    private Stack<BattleState> TurnStateStack = new Stack<BattleState>();
    public BattleState TurnState { get { return CurrentTurn.State; } set { CurrentTurn.State = value; } }

    private LinkedList<TurnState> TurnOrder;
    private LinkedListNode<TurnState> currentTurn;
    public TurnState CurrentTurn { get { return currentTurn.Value; } }

    public GameMap Map;
    public CursorControl Cursor;
    public CommandMenu Menu;
    public UnityEvent StateChanged;
    public event Action InRenderObject;

    void Awake()
    {
        instance = this;

        TurnOrder = new LinkedList<TurnState>( new TurnState[] { new PlayerTurnState() } );
        currentTurn = TurnOrder.First;
        currentTurn.Value.Enter( this );
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTurn.Update( this );
    }

    void OnRenderObject()
    {
        if ( InRenderObject != null )
            InRenderObject();
    }

    public void EndTurn()
    {
        CurrentTurn.Exit( this );

        LinkedListNode<TurnState> nextTurn = currentTurn.Next;
        if ( nextTurn == null )
            currentTurn = TurnOrder.First;
        else
            currentTurn = nextTurn;

        CurrentTurn.Enter( this );
    }
}

public interface IPlayerState
{
    void Update( BattleSystem state );
    void Enter( BattleSystem state );
    void Exit( BattleSystem state );
}

public interface ICommand
{
    void Execute();
}

public interface IUndoCommand : ICommand
{
    void Undo();
}

public struct UndoCommandAction : IUndoCommand
{
    private Action execute;
    private Action undo;

    public UndoCommandAction( Action execute, Action undo )
    {
        this.execute = execute;
        this.undo = undo;
    }

    public void Execute()
    {
        execute();
    }

    public void Undo()
    {
        undo();
    }
}