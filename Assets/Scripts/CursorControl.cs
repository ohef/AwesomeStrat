using UnityEngine;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Map;
using System;

[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
public class CursorControl : MonoBehaviour
{

    private enum CursorState
    {
        Moving,
        Stationary
    }

    public GameMap Map;
    public GameTile CurrentTile;

    private CursorState state = CursorState.Stationary;

    public Camera cursorCamera;

    public delegate void CursorMovedHandler( Vector3 oldPosition, Vector3 newPositon );
    public event CursorMovedHandler CursorMoved;

    private Vector3 CursorPosition
    {
        get { return transform.localPosition; }
        set
        {
            var oldlocalPosition = transform.localPosition;
            transform.localPosition = value;
            if ( CursorMoved != null )
                CursorMoved( oldlocalPosition, transform.localPosition );
        }
    }

    #region UnityMonoBehaviourFunctions

    void Awake()
    {
    }

    void Start()
    {
        cursorCamera.transform.LookAt( this.transform );
        CurrentTile = Map[ 0, 0 ];
    }
    #endregion

    public void MoveCursor( Vector2Int direction )
    {
        if ( direction.x != 0 || direction.y != 0 )
            switch ( state )
            {
                case CursorState.Stationary:
                    Vector2Int to = CurrentTile.Position + direction;
                    if ( Map.OutOfBounds( to ) == false )
                    {
                        state = CursorState.Moving;
                        CurrentTile = Map[ to ];
                        StartCoroutine( MotionTweenMap( to, 0.15f ) );
                    }
                    break;
                case CursorState.Moving:
                    break;
            }
    }

    IEnumerator MotionTweenMap( Vector2Int to, float seconds )
    {
        Vector3 oldPosition = CursorPosition;
        Vector3 updatedPosition = new Vector3( to.x, oldPosition.y, to.y );

        float rate = 1.0f / seconds;
        for ( float i = 0 ; i < 1.0f ; i += Time.deltaTime * rate )
        {
            CursorPosition = new Vector3(
            Mathf.Lerp( oldPosition.x, updatedPosition.x, i ),
            oldPosition.y,
            Mathf.Lerp( oldPosition.z, updatedPosition.z, i ) );
            yield return null;
        }

        CursorPosition = updatedPosition;

        state = CursorState.Stationary;
        yield return null;
    }
}
