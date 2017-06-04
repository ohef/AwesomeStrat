using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.General.DataStructures;

namespace Assets.General.UnityExtensions
{
    public static class Vector2IntExt
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public static Vector2 ToVector2( this Vector2Int v )
        {
            return new Vector2( v.x, v.y );
        }

        public static Vector3 ToVector3( this Vector2Int v, Axis axis = Axis.Z, float axisVal = 0 )
        {
            switch ( axis )
            {
                case Axis.X:
                    return new Vector3( axisVal, v.x, v.y );
                case Axis.Y:
                    return new Vector3( v.x, axisVal, v.y );
                case Axis.Z:
                    return new Vector3( v.x, v.y, axisVal );
                default:
                    return new Vector3( v.x, axisVal, v.y );
            }
        }
    }
    
    public static class Vector3Ext
    {
        public static Vector2Int ToVector2Int( this Vector2 v )
        {
            return new Vector2Int( ( int )v.x, ( int )v.y );
        }

        public static Vector2 ToVector2( this Vector3 v )
        {
            return new Vector2( v.x, v.y );
        }
    }
}
