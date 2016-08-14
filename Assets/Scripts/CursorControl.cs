using UnityEngine;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Map;
using System;

namespace Assets.Map
{
    [RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
    public class CursorControl : MonoBehaviour
    {
        public GameMap Map;
        public GameTile CurrentTile;

        public bool Moving = false;

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

        public void ShiftCursor( Vector2Int direction )
        {
            MoveCursor( CurrentTile.Position + direction );
        }

        public void MoveCursor( Vector2Int to )
        {
            if ( Moving == false )
                if ( Map.OutOfBounds( to ) == false )
                {
                    Moving = true;
                    CurrentTile = Map[ to ];
                    StartCoroutine( MotionTweenMap( to, 0.15f ) );
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

            Moving = false;
            yield return null;
        }
    }
}