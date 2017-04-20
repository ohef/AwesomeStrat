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

        var firstunit = Map.UnitPos.FirstOrDefault();
        if ( firstunit.Value == null ) return;

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
            MoveCursor( updatedPosition );
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
            StartCoroutine( CursorMotion( CustomAnimation.MotionTweenLinear( this.transform, to.ToVector3( Vector2IntExt.Axis.Z ), 0.08f ) ) );
        }
    }

    private IEnumerator CursorMotion( IEnumerator tweener )
    {
        IsMoving = true;
        yield return tweener;
        IsMoving = false;
    }
}

public class CursorEventData : BaseEventData
{
    public CursorControl Cursor;
    public Vector2Int LastPosition;

    public CursorEventData( EventSystem eventSystem ) : base( eventSystem )
    {
    }
}

interface IMapCursorEnter : IEventSystemHandler
{
    void CursorEnter( CursorEventData data );
}

interface IMapCursorExit : IEventSystemHandler
{
    void CursorExit( CursorEventData data );
}