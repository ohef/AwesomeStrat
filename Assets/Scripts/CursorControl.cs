using UnityEngine;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Linq;

namespace Assets.Map
{
    [RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
    public class CursorControl : MonoBehaviour
    {
        public GameMap Map;
        public GameTile CurrentTile;

        public Camera cursorCamera;

        #region UnityMonoBehaviourFunctions

        void Awake() { }

        void Start()
        {
            cursorCamera.transform.LookAt( this.transform );
            Unit firstunit = default( Unit );
            CurrentTile = Map.FirstOrDefault( tile => Map.UnitGametileMap.TryGetValue( tile, out firstunit ) );
            MoveCursor( CurrentTile.Position );
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
                return updatedPosition;
            else return CurrentTile.Position;
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