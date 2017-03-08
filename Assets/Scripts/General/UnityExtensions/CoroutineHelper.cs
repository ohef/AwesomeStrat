using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.General.UnityExtensions
{
    public static class CoroutineHelper
    {
        public static IEnumerator AddBefore( IEnumerator enumerator, Action DoBefore )
        {
            DoBefore();
            yield return enumerator;
        }

        public static IEnumerator AddActions( IEnumerator enumerator, Action DoBefore, Action DoAfter )
        {
            DoBefore();
            yield return enumerator;
            DoAfter();
        }

        public static IEnumerator AddAfter( IEnumerator enumerator, Action DoAfter )
        {
            yield return enumerator;
            DoAfter();
        }
    }
}
