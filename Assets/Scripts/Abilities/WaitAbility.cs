using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class WaitAbility : IAbility
{
    public static WaitAbility Instance = new WaitAbility();

    public string Name { get { return "Wait"; } }
    public Image AbliityImage { get { return null; } }
    public Color TileColor { get { return default( Color ); } }

    public T Accept<T>( IAbilityVisitor<T> visitor )
    {
        return visitor.Visit( this );
    }

    public bool Useable( Unit user )
    {
        return true;
    }
}