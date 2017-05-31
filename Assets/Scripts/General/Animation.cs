using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.General
{
    public static class CustomAnimation
    {
        public static IEnumerator InterpolateToPoint( Transform from, Vector3 to, float seconds )
        {
            float rate = 1.0f / seconds;
            for ( float i = 0 ; i <= 1.0f ; i += Time.deltaTime * rate )
            {
                from.localPosition =
                    new Vector3(
                    Mathf.Lerp( from.localPosition.x, to.x, i ),
                    Mathf.Lerp( from.localPosition.y, to.y, i ),
                    Mathf.Lerp( from.localPosition.z, to.z, i ) );
                yield return null;
            }
            from.localPosition = to;
        }

        /// <summary>
        /// Moves a transform between a set of points at a linear speed
        /// </summary>
        /// <param name="toInterp">The transform of the given object to move</param>
        /// <param name="nodesToPass">List of nodes to pass the transform through</param>
        /// <param name="seconds">The time taken to get through each pair of points</param>
        public static IEnumerator InterpolateBetweenPoints( Transform toInterp, IList<Vector3> nodesToPass, float seconds )
        {
            if ( nodesToPass.Count > 1 )
            {
                float rate = 1.0f / seconds;
                for ( float i = 0 ; i <= nodesToPass.Count - 1 ; i += Time.deltaTime * rate )
                {
                    int currentNode = ( int )Math.Truncate( i );
                    float t = i - currentNode;
                    Vector3 a = nodesToPass[ currentNode ];
                    Vector3 b = nodesToPass[ currentNode + 1 ];
                    toInterp.localPosition = Vector3.Lerp( a, b, t );
                    yield return null;
                }
                toInterp.localPosition = nodesToPass.Last();
            }
        }

        public static IEnumerator InterpolateBetweenPointsDecoupled( Transform toMove, Transform toRotate, IList<Vector3> nodesToPass, float seconds )
        {
            if ( nodesToPass.Count > 1 )
            {
                float rate = 1.0f / seconds;
                for ( float i = 0 ; i <= nodesToPass.Count - 1 ; i += Time.deltaTime * rate )
                {
                    int currentNode = ( int )Math.Truncate( i );
                    float t = i - currentNode;
                    Vector3 a = nodesToPass[ currentNode ];
                    Vector3 b = nodesToPass[ currentNode + 1 ];
                    toRotate.rotation = Quaternion.LookRotation( b - a );
                    toMove.localPosition = Vector3.Lerp( a, b, t );
                    yield return null;
                }
                toMove.localPosition = nodesToPass.Last();
                //yield return null;
            }
        }

        public static IEnumerator InterpolateValue( float a, float b, float seconds, Action<float> setter )
        {
            float rate = 1.0f / seconds;
            for ( float i = 0 ; i <= 1 ; i += Time.deltaTime * rate )
            {
                setter( Mathf.Lerp( a, b, i ) );
                yield return null;
            }
        }
    }
}
