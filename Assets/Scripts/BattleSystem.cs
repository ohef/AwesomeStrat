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

    private LinkedList<PlayerTurnController> TurnOrder;
    private LinkedListNode<PlayerTurnController> currentTurn;

    public PlayerTurnController CurrentTurn { get { return currentTurn.Value; } }
    public BattleState TurnState { get { return CurrentTurn.State; } set { CurrentTurn.State = value; } }

    public GameMap Map;
    public CursorControl Cursor;
    public CommandMenu Menu;
    public Transform UnitLayer;
    public Transform TileLayer;
    public UnityEvent StateChanged;

    void Awake()
    {
        instance = this;

        TurnOrder = new LinkedList<PlayerTurnController>( new PlayerTurnController[] {
            new PlayerTurnController( unit => unit.PlayerOwner == 0, new Color(1.0f, 0.5f, 0.5f) ),
            new PlayerTurnController( unit => unit.PlayerOwner == 1, new Color(0.5f, 1.0f, 0.5f) ),
            new PlayerTurnController( unit => unit.PlayerOwner == 2, new Color(0.5f, 0.5f, 1.0f) )
        } );
        currentTurn = TurnOrder.First;
        currentTurn.Value.Enter( this );
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTurn.Update( this );
    }

    public void EndTurn()
    {
        CurrentTurn.Exit( this );

        LinkedListNode<PlayerTurnController> nextTurn = currentTurn.Next;
        if ( nextTurn == null )
            currentTurn = TurnOrder.First;
        else
            currentTurn = nextTurn;

        CurrentTurn.Enter( this );
    }
}