using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Linq;
using System;
using System.Collections.Generic;

public class CursorControl : MonoBehaviour
{
    public GameMap Map;
    public Camera CursorCamera;
    public Queue<Action> MoveCommands = new Queue<Action>();
    public UnityEvent CursorMoved;

    private Vector2Int m_CurrentPosition;
    public Vector2Int CurrentPosition
    {
        get
        {
            return m_CurrentPosition;
        }

        set
        {
            if ( value != m_CurrentPosition )
            {
                m_CurrentPosition = value;
                if ( CursorMoved != null )
                    CursorMoved.Invoke();
            }
        }
    }

    private bool IsMoving = false;

    void Awake()
    {
        if ( CursorMoved == null )
            CursorMoved = new UnityEvent();
    }

    void Start()
    {
        CursorCamera.transform.LookAt( this.transform );
        Unit firstunit = default( Unit );
        CurrentPosition = Map.UnitPos[ Map.UnitPos.First().Key ];
        MoveCursor( CurrentPosition );
    }

    public Unit GetCurrentUnit()
    {
        Unit unitThere;
        Map.UnitPos.TryGetValue( this.CurrentPosition, out unitThere );
        return unitThere;
    }

    public void UpdateAction()
    {
        Vector2Int inputVector = Vector2IntExt.GetInputAsDiscrete();
        ShiftCursor( inputVector );
    }

    public void Update()
    {
        if ( MoveCommands.Count == 0 == false && IsMoving == false )
        {
            MoveCommands.Dequeue()();
        }
    }

    /// <summary>
    /// Shifts the cursor according to directional vector, returns the updated position if successful
    /// else, returns the unmodified position.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public void ShiftCursor( Vector2Int direction )
    {
        if ( direction.AbsoluteNormal() != 0 )
        {
            Vector2Int updatedPosition = CurrentPosition + direction;
            MoveCommands.Enqueue( () => MoveCursor( updatedPosition ) );
        }
    }

    /// <summary>
    /// Moves the cursor to a position on the map, if successful, returns true; else false. 
    /// e.g. hit's the edge of the map
    /// </summary>
    /// <param name="to"></param>
    /// <returns></returns>
    public void MoveCursor( Vector2Int to )
    {
        if ( Map.IsOutOfBounds( to ) == false )
        {
            CurrentPosition = to;
            StartCoroutine( CursorMotion( CustomAnimation.MotionTweenLinear( this.transform, to.ToVector3(), 0.08f ) ) );
        }
    }

    private IEnumerator CursorMotion( IEnumerator tweener )
    {
        IsMoving = true;
        yield return tweener;
        IsMoving = false;
    }
}