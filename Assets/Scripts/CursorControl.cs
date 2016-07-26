using UnityEngine;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Map;
using System;

[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
public class CursorControl : MonoBehaviour {

    private enum CursorState
    {
        Moving,
        Stationary
    }

    private CursorState state = CursorState.Stationary;
    private MapCursor internalCursor;
    public Tile CurrentTile {
        get
        {
            if ( state != CursorState.Moving )
                return internalCursor.currentTile;
            return null;
        }
    }

    public GameMap map;
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
                CursorMoved( oldlocalPosition, transform.localPosition);
        }
    }

    #region UnityMonoBehaviourFunctions

    void Awake()
    {
        internalCursor = new MapCursor( map.MapInternal );
        cursorCamera.transform.LookAt( this.transform );
    }

    void Start() { }

    void Update() { }
    #endregion

    public void MoveCursor( Vector2Int direction )
    {
        if ( direction.x != 0 || direction.y != 0 )
            switch ( state )
            {
                case CursorState.Stationary:
                    state = CursorState.Moving;
                    StartCoroutine( MoveCursorDiscreteAnim( internalCursor.MoveCursor( internalCursor.currentLocation + direction ) ) );
                    break;
                case CursorState.Moving:
                    break;
            }
    }

    IEnumerator MoveCursorDiscreteAnim( Vector2Int to )
    {
        Vector3 oldPosition = CursorPosition;
        Vector3 updatedPosition = new Vector3( to.x, oldPosition.y, to.y );

        for ( float i = 0 ; i < 0.99f ; i+=0.33f )
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
