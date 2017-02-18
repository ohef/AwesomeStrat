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
        public Camera CursorCamera;

        private GameTile m_CurrentTile;
        public GameTile CurrentTile
        {
            get
            {
                return m_CurrentTile;
            }

            set
            {
                if ( value != m_CurrentTile )
                {
                    m_CurrentTile = value;
                    if ( CursorMoved != null )
                        CursorMoved( m_CurrentTile );
                }
            }
        }

        public event Action<GameTile> CursorMoved;

        #region UnityMonoBehaviourFunctions

        void Awake() { }

        void Start()
        {
            CursorCamera.transform.LookAt( this.transform );
            Unit firstunit = default( Unit );
            CurrentTile = Map.FirstOrDefault( tile => Map.UnitGametileMap.TryGetValue( tile, out firstunit ) );
            CurrentTile = CurrentTile == null ? Map[ 0, 0 ] : CurrentTile;
            MoveCursor( CurrentTile.Position );
        }

        public void UpdateAction()
        {
            int vertical = ( Input.GetButtonDown( "Up" ) ? 1 : 0 ) + ( Input.GetButtonDown( "Down" ) ? -1 : 0 );
            int horizontal = ( Input.GetButtonDown( "Left" ) ? -1 : 0 ) + ( Input.GetButtonDown( "Right" ) ? 1 : 0 );
            var inputVector = new Vector2Int( horizontal, vertical );
            if ( vertical != 0 || horizontal != 0 )
            {
                ShiftCursor( inputVector );
            }
        }
        #endregion

        /// <summary>
        /// Shifts the cursor according to directional vector, returns the updated position if successful
        /// else, returns the unmodified position.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public void ShiftCursor( Vector2Int direction )
        {
            Vector2Int updatedPosition = CurrentTile.Position + direction;
            MoveCursor( updatedPosition );
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
                CurrentTile = Map[ to ];
                StartCoroutine( CustomAnimation.MotionTweenLinear( this.transform, to.ToVector3(), 0.15f ) );
            }
        }
    }
}