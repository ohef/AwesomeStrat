using System;
using UnityEngine;
using UnityEngine.UI;

public abstract partial class Ability : ScriptableObject
{
    public string Name;
    public Image AbliityImage;
    public Color TileColor;

    public int GreenRequirement;
    public int BlueRequirement ;
    public int RedRequirement  ;
    public int WhiteRequirement;
    public int BlackRequirement;

    public bool Useable( Unit user )
    {
        return
            user.Green >= GreenRequirement &&
            user.Red   >= RedRequirement &&
            user.Blue  >= BlueRequirement &&
            user.Black >= BlackRequirement &&
            user.White >= WhiteRequirement;
    }
}

public enum AbilityTargets
{
    All,
    Enemy,
    Friendly
}