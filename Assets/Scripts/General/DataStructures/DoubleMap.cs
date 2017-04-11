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

[Serializable]
public class DoubleDictionary<A, B> : IEnumerable<KeyValuePair<A, B>>
{
    public Dictionary<A, B> AtoBDict; //A -> B Don't ever set this, should be used to enumerate
    public Dictionary<B, A> BtoADict; //B -> A Don't ever set this, should be used to enumerate

    public DoubleDictionary()
    {
        AtoBDict = new Dictionary<A, B>();
        BtoADict = new Dictionary<B, A>();
    }

    //TODO: This code could be erroneous, no testing gotta go go go
    public void Add( A a, B b )
    {
        A removeA = default(A);
        B removeB = default(B);

        if ( AtoBDict.TryGetValue( a, out removeB ) )
        {
            AtoBDict.Remove( a );
            if ( BtoADict.TryGetValue( removeB, out removeA ) )
                BtoADict.Remove( removeB );
            else { throw new System.Exception( "Whoah Dictionary was not updated correctly" ); }
        }

        if ( BtoADict.TryGetValue( b, out removeA ) )
        {
            BtoADict.Remove( b );
            if ( AtoBDict.TryGetValue( removeA, out removeB ) )
                AtoBDict.Remove( removeA );
            else { throw new System.Exception( "Whoah Dictionary was not updated correctly" ); }
        }

        AtoBDict[ a ] = b;
        BtoADict[ b ] = a;
    }

    public void Remove( A a )
    {
        var b = AtoBDict[ a ];
        AtoBDict.Remove( a );
        BtoADict.Remove( b );
    }

    public void Remove( B b )
    {
        var a = BtoADict[ b ];
        BtoADict.Remove( b );
        AtoBDict.Remove( a );
    }

    public void Clear()
    {
        AtoBDict.Clear();
        BtoADict.Clear();
    }

    public B this[ A a ]
    {
        get { return AtoBDict[ a ]; }
    }

    public A this[ B b ]
    {
        get { return BtoADict[ b ]; }
    }

    public bool TryGetValue( A a, out B b )
    {
        return AtoBDict.TryGetValue( a, out b );
    }

    public bool TryGetValue( B b, out A a )
    {
        return BtoADict.TryGetValue( b, out a );
    }

    IEnumerator<KeyValuePair<A, B>> IEnumerable<KeyValuePair<A, B>>.GetEnumerator()
    {
        return AtoBDict.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return AtoBDict.GetEnumerator();
    }
}
