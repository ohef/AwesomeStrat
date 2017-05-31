using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.General
{
    public static class CustomMath
    {
        public static T ClampNumber<T>( T i, T lowerBound, T upperBound ) where T : IComparable<T>
        {
            i = i.CompareTo( lowerBound ) < 0 ? lowerBound : i;
            i = i.CompareTo( upperBound ) > 0 ? upperBound : i;
            return i;
        }

        public static bool IsOverBound<T>( T i, T lowerBound, T upperBound ) where T : IComparable<T>
        {
            return i.CompareTo( lowerBound ) < 0 || i.CompareTo( upperBound ) > 0;
        }

        /// <summary>
        ///  Works like the python range function for simple iteration over a list of ints
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static IEnumerable<int> Range( int start, int end, int step = 1 )
        {
            for ( int i = start ; i <= end ; i += step )
            {
                yield return i;
            }
        }
    }
}
