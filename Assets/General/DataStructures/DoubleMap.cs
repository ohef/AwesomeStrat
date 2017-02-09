using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Just a simple one to one map with two dictionaries, probably 
/// don't want to implement a double interface so this is just going to be a new type of data container
/// </summary>
/// <typeparam name="A"></typeparam>
/// <typeparam name="B"></typeparam>

[System.Serializable]
public class DoubleDictionary<A, B> 
{
    private Dictionary<A, B> AtoB;
    private Dictionary<B, A> BtoA;

    public DoubleDictionary()
    {
        AtoB = new Dictionary<A, B>();
        BtoA = new Dictionary<B, A>();
    }

    public void Add( A a, B b )
    {
        A removeA = default(A);
        B removeB = default(B);

        if ( AtoB.TryGetValue( a, out removeB ) )
        {
            AtoB.Remove( a );
            if ( BtoA.TryGetValue( removeB, out removeA ) )
                BtoA.Remove( removeB );
            else { throw new System.Exception( "Whoah Dictionary was not updated correctly" ); }
        }

        AtoB[ a ] = b;
        BtoA[ b ] = a;
    }

    public B this[ A a ]
    {
        get { return AtoB[ a ]; }
    }

    public A this[ B b ]
    {
        get { return BtoA[ b ]; }
    }

    public bool TryGetValue( A a, out B b )
    {
        return AtoB.TryGetValue( a, out b );
    }

    public bool TryGetValue( B b, out A a )
    {
        return BtoA.TryGetValue( b, out a );
    }
}
