using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : ScriptableObject, IAbility, IAbilityStatRequirements
{
    [SerializeField]
    private string _Name;
    [SerializeField]
    private Image _AbliityImage;
    [SerializeField]
    private Color _TileColor;

    public Image AbliityImage { get { return _AbliityImage; }  protected set { _AbliityImage = value; } }
    public Color TileColor { get { return _TileColor; }  protected set { _TileColor = value; } }
    public string Name { get { return _Name; } protected set { _Name = value; } }

    public int GreenRequirement { get; set; }
    public int BlueRequirement { get; set; }
    public int RedRequirement { get; set; }
    public int WhiteRequirement { get; set; }
    public int BlackRequirement { get; set; }

    public abstract T Accept<T>( IAbilityVisitor<T> visitor );

    public bool Useable( Unit user )
    {
        return
            user.Green >= GreenRequirement &&
            user.Red   >= RedRequirement   &&
            user.Blue  >= BlueRequirement  &&
            user.Black >= BlackRequirement &&
            user.White >= WhiteRequirement;
    }
}

public interface IAbilityStatRequirements
{
    int GreenRequirement { get; set; }
    int BlueRequirement { get; set; }
    int RedRequirement { get; set; }
    int WhiteRequirement { get; set; }
    int BlackRequirement { get; set; }
}

public interface IAbility
{
    string Name { get; }
    Image AbliityImage { get; }
    Color TileColor { get; }

    bool Useable( Unit user );

    T Accept<T>( IAbilityVisitor<T> visitor );
}

public enum AbilityTargets
{
    All,
    Enemy,
    Friendly
}