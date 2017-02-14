using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Just a simple one to one map with two dictionaries, probably 
/// don't want to implement a double interface so this is just going to be a new type of data container
/// </summary>
/// <typeparam name="A"></typeparam>
/// <typeparam name="B"></typeparam>

[System.Serializable]
public class DoubleDictionary<A, B> : IEnumerable<A>
{
    public Dictionary<A, B> AtoB; //A -> B Don't ever set this, should be used to enumerate
    public Dictionary<B, A> BtoA; //B -> A Don't ever set this, should be used to enumerate

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

    public void Remove( A a )
    {
        var b = AtoB[ a ];
        AtoB.Remove( a );
        BtoA.Remove( b );
    }
    
    public void Remove(B b)
    {
        var a = BtoA[ b ];
        BtoA.Remove( b );
        AtoB.Remove( a );
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

    public IEnumerator<A> GetEnumerator()
    {
        return AtoB.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return AtoB.Keys.GetEnumerator();
    }
}
