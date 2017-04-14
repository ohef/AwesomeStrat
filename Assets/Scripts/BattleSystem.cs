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

public class BattleSystem : MonoBehaviour
{
    private static BattleSystem instance;
    public static BattleSystem Instance { get { return instance; } }

    private LinkedList<TurnController> TurnOrder;
    private LinkedListNode<TurnController> currentTurn;

    public TurnController CurrentTurn { get { return currentTurn.Value; } }

    public GameMap Map;
    public CursorControl Cursor;
    public CommandMenu Menu;
    public Transform UnitLayer;
    public Transform TileLayer;
    public UnityEvent StateChanged;

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
        currentTurn.Value.enabled = true;
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
                    CoroutineHelper.AddActions( 
                        CustomAnimation.InterpolateBetweenPointsDecoupled( unit.transform,
                        unit.transform.FindChild( "Model" ),
                        path.Select( x => Map.TilePos[ x ].GetComponent<Transform>().localPosition ).ToList(), 0.22f ),
                        () => unit.GetComponentInChildren<Animator>().SetBool( "Moving", true ),
                        () => unit.GetComponentInChildren<Animator>().SetBool( "Moving", false ) ) );

                Map.PlaceUnit( unit, targetPosition );
            },
            delegate
            {
                Vector2Int initialPosition = path.First();
                unit.GetComponentInChildren<Animator>().SetBool( "Moving", false );
                unit.StopAllCoroutines();

                Map.PlaceUnit( unit, initialPosition );
                unit.transform.position = Map.TilePos[ initialPosition ].transform.position;

                Cursor.MoveCursor( initialPosition);
                Map.GetComponent<MapDecorator>().ShowUnitMovement( unit );
            } );
    }
}