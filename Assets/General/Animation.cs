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
        public static IEnumerator MotionTweenLinear( Transform from, Vector3 to, float seconds )
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

            //from.transform.localPosition = to;
            //yield return null;
        }

        public static IEnumerator MotionTweenLinear( Vector3 from, Vector3 to, Action<Vector3> setter, float seconds )
        {
            float rate = 1.0f / seconds;
            for ( float i = 0 ; i < 1.0f ; i += Time.deltaTime * rate )
            {
                setter(
                    new Vector3(
                    Mathf.Lerp( from.x, to.x, i ),
                    Mathf.Lerp( from.y, to.y, i ),
                    Mathf.Lerp( from.z, to.z, i ) )
                );
                yield return null;
            }
            setter( to );

            yield return null;
        }
    }
}
