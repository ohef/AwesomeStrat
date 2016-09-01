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

        private Vector3 CursorPosition
        {
            get { return transform.localPosition; }
            set
            {
                var oldlocalPosition = transform.localPosition;
                transform.localPosition = value;
            }
        }

        #region UnityMonoBehaviourFunctions

        void Awake() { }

        void Start()
        {
            cursorCamera.transform.LookAt( this.transform );
            CurrentTile = Map[ Map.Width / 2, Map.Height / 2 ];
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
                //StartCoroutine( CustomAnimation.MotionTweenLinear( CursorPosition, to.ToVector3(), SetThisPosition, 0.15f ) );
                StartCoroutine( CustomAnimation.MotionTweenLinear( this.transform, to.ToVector3(), 0.15f ) );
            }
        }
    }
}