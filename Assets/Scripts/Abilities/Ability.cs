using UnityEngine.UI;

public abstract partial class Ability
{
    public Unit Owner;
    public string Name;
    public Image AbliityImage;
}

public enum AbilityTargets
{
    All,
    Enemy,
    Friendly
}