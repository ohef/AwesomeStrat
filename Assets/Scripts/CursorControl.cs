using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Linq;
using System;

namespace Assets.Map
{
    [RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
    public class CursorControl : MonoBehaviour
    {
        public GameMap Map;
        public GameTile CurrentTile;
        public Camera CursorCamera;
        public bool MovementEnabled;
        public event Action<GameTile> CursorMoved;

        #region UnityMonoBehaviourFunctions

        void Awake() { }

        void Start()
        {
            CursorCamera.transform.LookAt( this.transform );
            Unit firstunit = default( Unit );
            CurrentTile = Map.FirstOrDefault( tile => Map.UnitGametileMap.TryGetValue( tile, out firstunit ) );
            CurrentTile = CurrentTile == null ? Map[ 0, 0 ] : CurrentTile;
            MoveCursorEventTrigger( CurrentTile.Position );
        }

        void Update()
        {
            int vertical = ( Input.GetButtonDown( "Up" ) ? 1 : 0 ) + ( Input.GetButtonDown( "Down" ) ? -1 : 0 );
            int horizontal = ( Input.GetButtonDown( "Left" ) ? -1 : 0 ) + ( Input.GetButtonDown( "Right" ) ? 1 : 0 );
            var inputVector = new Vector2Int( horizontal, vertical );
            if ( vertical != 0 || horizontal != 0 )
            {
                if ( MovementEnabled == true )
                    MoveCursorEventTrigger( inputVector );
            }
        }
        #endregion

        /// <summary>
        /// Shifts the cursor according to directional vector, returns the updated position if successful
        /// else, returns the unmodified position.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector2Int ShiftCursor( Vector2Int direction )
        {
            Vector2Int updatedPosition = CurrentTile.Position + direction;
            if ( MoveCursor( updatedPosition ) )
            {
                if ( CursorMoved != null )
                    CursorMoved( CurrentTile );
                return updatedPosition;
            }
            else return CurrentTile.Position;
        }

        private void MoveCursorEventTrigger(Vector2Int inputVector)
        {
            ShiftCursor( inputVector );
            if ( CursorMoved != null )
                CursorMoved( CurrentTile );
        }

        /// <summary>
        /// Moves the cursor to a position on the map, if successful, returns true; else false. 
        /// e.g. hit's the edge of the map
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool MoveCursor( Vector2Int to )
        {
            if ( Map.IsOutOfBounds( to ) == false )
            {
                CurrentTile = Map[ to ];
                StartCoroutine( CustomAnimation.MotionTweenLinear( this.transform, to.ToVector3(), 0.15f ) );
                return true;
            }
            else return false;
        }
    }
}