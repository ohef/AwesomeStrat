using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.General
{
    /// <summary>
    /// 
    /// </summary>
    public static class Functional
    {
        static Func<Argument, Result> 
            Memoize<Argument, Result>( this Func<Argument, Result> function )
        {
            var dictionary = new Dictionary<Argument, Result>();
            return arg =>
            {
                Result res;
                if ( !dictionary.TryGetValue( arg, out res ) )
                {
                    res = function( arg );
                    dictionary.Add( arg, res );
                }
                return res;
            };
        }
    }
}
