using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.General.DataStructures
{
    [System.Serializable]
    public struct Vector2Int : IEquatable<Vector2Int>, IEqualityComparer<Vector2Int>
    {
        public int x;
        public int y;

        public Vector2Int( int x, int y )
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals( Vector2Int other )
        {
            return this.x == other.x && this.y == other.y;
        }

        public static Vector2Int operator -( Vector2Int a, Vector2Int b )
        {
            return new Vector2Int( a.x - b.x, a.y - b.y );
        }

        public static Vector2Int operator +( Vector2Int a, Vector2Int b )
        {
            return new Vector2Int( a.x + b.x, a.y + b.y );
        }

        public static Vector2Int operator *( Vector2Int a, int b )
        {
            return new Vector2Int( a.x * b, a.y * b );
        }

        public int AbsoluteNormal()
        {
            return Math.Abs( this.x ) + Math.Abs( this.y );
        }

        public override string ToString()
        {
            return string.Format( "({0},{1})", x, y );
        }

        public bool Equals( Vector2Int x, Vector2Int y )
        {
            return x.x == y.x && y.y == x.y;
        }

        //Assuming that vector2ints don't get literally huge? :) 
        public int GetHashCode( Vector2Int obj )
        {
            return obj.x + 500 * obj.y;
        }

        private static Vector2Int _Up = new Vector2Int( 0, 1 );
        public static Vector2Int Up { get { return _Up; } }

        private static Vector2Int _Down = new Vector2Int( 0, -1 );
        public static Vector2Int Down { get { return _Down; } }

        private static Vector2Int _Left = new Vector2Int( -1, 0 );
        public static Vector2Int Left { get { return _Left; } }

        private static Vector2Int _Right = new Vector2Int( 1, 0 );
        public static Vector2Int Right { get { return _Right; } }

        private static Vector2Int _Zero = new Vector2Int( 1, 0 );
        public static Vector2Int Zero { get { return _Zero; } }
    }
}
