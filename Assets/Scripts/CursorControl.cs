using UnityEngine;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System;

namespace Assets.Map
{
    [RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
    public class CursorControl : MonoBehaviour
    {
        public GameMap Map;
        public GameTile CurrentTile;

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
            if ( Map.OutOfBounds( to ) == false )
            {
                CurrentTile = Map[ to ];
                StartCoroutine( CustomAnimation.MotionTweenLinear( CursorPosition, to.ToVector3(), SetPosition, 0.15f ) );
            }
        }

        void SetPosition( Vector3 toSetTo )
        {
            CursorPosition = toSetTo;
        }

    }
}