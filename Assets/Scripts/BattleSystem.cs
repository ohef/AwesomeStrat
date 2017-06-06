using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Assets.General.DataStructures;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Assets.General.UnityExtensions;
using Assets.General;

public class BattleSystem : MonoBehaviour, ISubmitHandler, IMoveHandler, ICancelHandler, IPointerDownHandler, IPointerEnterHandler 
{
    private static BattleSystem instance;
    public static BattleSystem Instance { get { return instance; } }

    private LinkedList<TurnController> TurnOrder;
    private LinkedListNode<TurnController> currentTurn;

    public TurnController CurrentTurn { get { return currentTurn.Value; } }

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

    void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        TurnOrder = new LinkedList<TurnController>( 
            this.transform.GetComponentsInChildren<TurnController>()
            .OrderBy( x => x.PlayerNo ) );

        currentTurn = TurnOrder.First;
    }

    public void EndTurn()
    {
        CurrentTurn.ExitState();

        LinkedListNode<TurnController> nextTurn = currentTurn.Next;
        if ( nextTurn == null )
            currentTurn = TurnOrder.First;
        else
            currentTurn = nextTurn;

        CurrentTurn.EnterState();
    }

    public List<BattleState> StateList = new List<BattleState>();

    public T GetState<T>() where T : class
    {
        foreach ( var state in StateList )
        {
            if ( state is T )
                return state as T;
        }
        throw new Exception( "The given state could not be found!" );
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

    public void OnSubmit( BaseEventData eventData )
    {
        ExecuteEvents.Execute( CurrentTurn.gameObject, eventData, ExecuteEvents.submitHandler );
    }

    public void OnMove( AxisEventData eventData )
    {
        ExecuteEvents.Execute( CurrentTurn.gameObject, eventData, ExecuteEvents.moveHandler );
    }

    public void OnCancel( BaseEventData eventData )
    {
        ExecuteEvents.Execute( CurrentTurn.gameObject, eventData, ExecuteEvents.cancelHandler );
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        var tile = eventData.pointerEnter.GetComponent<GameTile>();
        if ( tile != null )
        {
            var tilePosition = Map.TilePos[ tile ];
            Map.GetComponent<MapDecorator>().CursorRenderer.ShadeAtPosition( tilePosition, true );
        }
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        var tile = eventData.pointerPress.GetComponent<GameTile>();
        if ( tile != null )
        {
            var tilePosition = Map.TilePos[ tile ];
            if ( Cursor.CurrentPosition == tilePosition )
                OnSubmit( eventData );
            else
                Cursor.MoveCursor( tilePosition );
        }

        EventSystem.current.SetSelectedGameObject( gameObject );
    }
}