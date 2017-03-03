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

        public static Vector3 ToVector3( this Vector2Int v, Axis axis = Axis.Y, float axisVal = 0 )
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

        public static Vector2Int GetInputAsDiscrete()
        {
            int vertical = ( Input.GetButtonDown( "Up" ) ? 1 : 0 ) + ( Input.GetButtonDown( "Down" ) ? -1 : 0 );
            int horizontal = ( Input.GetButtonDown( "Left" ) ? -1 : 0 ) + ( Input.GetButtonDown( "Right" ) ? 1 : 0 );
            var inputVector = new Vector2Int( horizontal, vertical );
            return inputVector;
        }
    }
}
