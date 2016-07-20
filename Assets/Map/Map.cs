using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.General.DataStructures;
using UnityEngine;

namespace Assets.Map
{
    [Serializable]
    public class Tile 
    {
        public Vector2Int Position;
        public Unit UnitOccupying;
        public int CostOfTraversal;
    }

    public class Map : IEnumerable
    {
        public Vector2Int MapSize;
        public Tile[,] m_TileMap;

        public Map( int x, int y ) : this( x, y, new Tile { CostOfTraversal = 1, Position = new Vector2Int(), UnitOccupying = null } )
        {
        }

        public Map( int x, int y, Tile defaultTile )
        {
            m_TileMap = new Tile[ x, y ];
            for ( int i = 0 ; i < x ; i++ )
                for ( int j = 0 ; j < y ; j++ )
                {
                    m_TileMap[ i, j ] = new Tile { Position = new Vector2Int( i, j ), CostOfTraversal = defaultTile.CostOfTraversal, UnitOccupying = null };
                }

            MapSize = new Vector2Int( x, y );
        }

        public Tile this[ int x, int y ]
        {
            get { return m_TileMap[ x, y ]; }
            set { m_TileMap[ x, y ] = value; }
        }

        //TODO: Error Handle plz
        public Tile this[Vector2Int v]
        {
            get { return m_TileMap[ v.x, v.y ]; }
            set { m_TileMap[ v.x, v.y ] = value; }
        }

        private bool CanMoveInto( Tile tile )
        {
            return tile.UnitOccupying == null;
        }

        private void SwapUnit( Tile a, Tile b )
        {
            if ( CanMoveInto( b ) == true )
            {
                b.UnitOccupying = a.UnitOccupying;
                a.UnitOccupying = null;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return m_TileMap.GetEnumerator();
        }

        private static bool OverBounded( int i, int lowerBound, int upperBound )
        {
            return i < lowerBound || i > upperBound;
        }

        private static int ClampInt( int i, int lowerBound, int upperBound )
        {
            i = i < lowerBound ? lowerBound : i;
            i = i > upperBound ? upperBound : i;
            return i;
        }

        public int ClampX( int i )
        {
            return ClampInt( i, 0, MapSize.x - 1 );
        }

        public int ClampY( int i )
        {
            return ClampInt( i, 0, MapSize.y - 1 );
        }

        public Vector2Int ClampPositionViaMap( Vector2Int toClamp )
        {
            var mapSize = MapSize;
            toClamp.x = ClampInt( toClamp.x, 0, mapSize.x - 1 );
            toClamp.y = ClampInt( toClamp.y, 0, mapSize.y - 1 );
            return toClamp;
        }

        public bool OutOfBounds( Vector2Int v )
        {
            return OverBounded( v.x, 0, MapSize.x - 1 ) || OverBounded( v.y, 0, MapSize.y - 1 );
        }
    }
}
