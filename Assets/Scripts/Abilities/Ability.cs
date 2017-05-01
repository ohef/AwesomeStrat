using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( Unit ) )]
public abstract partial class Ability : MonoBehaviour
{
    [HideInInspector]
    public Unit Owner;
    public string Name;
    public Image AbliityImage;
    public Color TileColor;

    public virtual void Awake()
    {
        Owner = this.GetComponent<Unit>();
    }
}

public enum AbilityTargets
{
    All,
    Enemy,
    Friendly
}