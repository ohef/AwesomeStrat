using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Map
{
    public class MapCursor
    {
        public Map map;
        public Tile currentTile;
        public Vector2Int currentLocation;

        public MapCursor( Map map, Vector2Int start = new Vector2Int() )
        {
            currentTile = map[ start.x, start.y ];
            currentLocation = start;
            this.map = map;
        }

        public Vector2Int MoveCursor( Vector2Int to )
        {
            if ( map.OutOfBounds( to ) == false )
            {
                this.currentLocation = to;
                currentTile = map[ to ];
            }

            return currentLocation;
        }
    }
}
