using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Assets.General.DataStructures;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Assets.Scripts.General;
using Assets.General;

public class BattleSystem : MonoBehaviour
    , IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static BattleSystem instance;
    public static BattleSystem Instance { get { return instance; } }

    LinearStateMachine<TurnController> TurnStateMachine;
    public TurnController CurrentTurn { get { return TurnStateMachine.Current; } }

    /// <summary>
    /// The single map object (singleton?) =)
    /// </summary>
    public GameMap Map;

    /// <summary>
    /// The single map cursor
    /// </summary>
    public CursorControl Cursor;

    /// <summary>
    /// TODO: This menu is used by all clients; probably should change it 
    /// </summary>
    public CommandMenu Menu;

    private List<BattleState> StateList;

    private GameObject CreateStateGameObj( params Type[] components )
    {
        GameObject obj = new GameObject( components[ 0 ].Name, components );
        obj.transform.parent = this.transform;
        return obj;
    }

    void Awake()
    {
        instance = this;

        StateList = new List<GameObject> {
            CreateStateGameObj( typeof( ChoosingUnitState ), typeof( ControlCursorState ) ),
            CreateStateGameObj( typeof( WhereToMoveState ), typeof( ControlCursorState ), typeof( CancelableState ) ),
            CreateStateGameObj( typeof( ChoosingUnitActionsState ), typeof( CancelableState) ),
            CreateStateGameObj( typeof( ChooseTargetsState ), typeof( CancelableState) ),
            CreateStateGameObj( typeof( TurnMenuState ), typeof( CancelableState) ) }
        .Select( obj => obj.GetComponent<BattleState>() ).ToList();

        foreach ( var state in StateList )
            state.BroadcastMessage( "Awake", SendMessageOptions.DontRequireReceiver );

        foreach ( var state in StateList )
            state.BroadcastMessage( "Start", SendMessageOptions.DontRequireReceiver );

        State = GetState<ChoosingUnitState>();
    }

    public void Start()
    {
        TurnStateMachine = new LinearStateMachine<TurnController>( 
            transform.GetComponentsInChildren<TurnController>().OrderBy( x => x.PlayerNo ) );
    }

    #region State Control
    public void EndTurn()
    {
        TurnStateMachine.Next();
    }

    public T GetState<T>() where T : class
    {
        foreach ( var state in StateList )
        {
            if ( state is T )
                return state as T;
        }
        throw new Exception( "The given state could not be found!" );
    }

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

    private void TransitionCalls( BattleState from, BattleState to )
    {
        from.Exit();
        to.Enter();
    }

    private Stack<IUndoCommand> Commands = new Stack<IUndoCommand>();
    private Stack<IUndoCommand> CommandsForReEnter = new Stack<IUndoCommand>();

    public void GoToPreviousState()
    {
        if ( State is ChoosingUnitState )
            return;

        TransitionCalls( StateStack.Pop(), State );
    }

    public UndoCommandAction CreateMoveCommand( IEnumerable<Vector2Int> path, Unit unit )
    {
        return new UndoCommandAction(
            delegate
            {
                Vector2Int targetPosition = path.Last();
                unit.StartCoroutine(
                        CustomAnimation.InterpolateBetweenPoints( unit.transform,
                        path.Select( x => Map.TilePos[ x ].GetComponent<Transform>().localPosition ).ToList(), 0.22f ) );

                Map.PlaceUnit( unit, targetPosition );
            },
            delegate
            {
                Vector2Int initialPosition = path.First();
                unit.StopAllCoroutines();

                Map.PlaceUnit( unit, initialPosition );
                unit.transform.position = Map.TilePos[ initialPosition ].transform.position;

                Cursor.MoveCursor( initialPosition );
            } );
    }

    public UndoCommandAction CreateMoveCommand( Vector2Int targetPosition, Unit unit )
    {
        Vector2Int initialPosition = Map.UnitPos[ unit ];

        //TODO: Transforms shouldn't be updated here as the underlying data structure is changed. Hopefully
        //I come up with something more intelligent
        return new UndoCommandAction(
            delegate
            {
                Map.PlaceUnit( unit, targetPosition );
                unit.transform.position = Map.TilePos[ targetPosition ].transform.position;
            },
            delegate
            {
                Map.PlaceUnit( unit, initialPosition );
                unit.transform.position = Map.TilePos[ initialPosition ].transform.position;
            } );
    }

    public void UndoEverything()
    {
        foreach ( var command in Commands )
            command.Undo();

        Commands.Clear();
    }

    public void GoToState( BattleState state, bool clearHistory = true )
    {
        TransitionCalls( StateStack.Pop(), state );
        if ( clearHistory == true ) ClearStateHistory();
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
        CurrentTurn.HasNotActed.Remove( unit );
    }

    public void RefreshTurn()
    {
        foreach ( var c in CommandsForReEnter )
            c.Undo();
        CommandsForReEnter.Clear();
    }
    #endregion

    #region Interface Implementations
    public void OnSubmit( BaseEventData eventData )
    {
        ExecuteEvents.Execute( State.gameObject, eventData, ExecuteEvents.submitHandler );
    }

    public void OnMove( AxisEventData eventData )
    {
        ExecuteEvents.Execute( State.gameObject, eventData, ExecuteEvents.moveHandler );
    }

    public void OnCancel( BaseEventData eventData )
    {
        ExecuteEvents.Execute( State.gameObject, eventData, ExecuteEvents.cancelHandler );
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        ExecuteEvents.Execute( State.gameObject, eventData, ExecuteEvents.pointerDownHandler );
    }

    public void OnBeginDrag( PointerEventData eventData )
    {
        ExecuteEvents.Execute( State.gameObject, eventData, ExecuteEvents.beginDragHandler );
    }

    public void OnDrag( PointerEventData eventData )
    {
        ExecuteEvents.Execute( State.gameObject, eventData, ExecuteEvents.dragHandler );
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        ExecuteEvents.Execute( State.gameObject, eventData, ExecuteEvents.endDragHandler );
    }
    #endregion
}