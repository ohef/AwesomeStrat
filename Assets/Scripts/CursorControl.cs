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

    void Awake()
    {
        if ( CursorMoved == null )
            CursorMoved = new UnityEvent();
    }

    void Start()
    {
        CursorCamera.transform.LookAt( this.transform );

        var firstunit = Map.UnitPos.FirstOrDefault();
        if ( firstunit.Key == null ) return;

        CurrentPosition = Map.UnitPos[ firstunit.Key ];

        MoveCursor( CurrentPosition );
    }

    public Unit GetCurrentUnit()
    {
        Unit unitThere;
        Map.UnitPos.TryGetValue( this.CurrentPosition, out unitThere );
        return unitThere;
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
            StartCoroutine( CustomAnimation.InterpolateToPoint( this.transform, to.ToVector3( Vector2IntExt.Axis.Z ), 0.08f ) );
        }
    }
}