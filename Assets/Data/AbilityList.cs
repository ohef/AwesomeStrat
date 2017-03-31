using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AbilityEditor
{
    [MenuItem("Assets/Create/Ability List")]
    public static AbilityList CreateAbilityList()
    {
        AbilityList asset = ScriptableObject.CreateInstance<AbilityList>();
        AssetDatabase.CreateAsset( asset, "Assets/AbilityList.asset" );
        AssetDatabase.SaveAssets();
        return asset;
    }

    [MenuItem("Assets/Create/Ability Item")]
    public static AbilityItem CreateAbilityItem()
    {
        AbilityItem asset = ScriptableObject.CreateInstance<AbilityItem>();
        AssetDatabase.CreateAsset( asset, "Assets/AbilityItem.asset" );
        AssetDatabase.SaveAssets();
        return asset;
    }
}

[Serializable]
public class AbilityList : ScriptableObject
{
    public List<AbilityItem> Abilities; 
}

[Serializable]
public class AbilityItem : ScriptableObject
{
    public static Dictionary<string, Func<Ability>> 
        NamesToAbilities = CreateDictionary();

    public static Dictionary<string, Func<Ability>> CreateDictionary()
    {
        var toret = new Dictionary<string, Func<Ability>>();
        toret[ "Attack" ] = () => new AttackAbility { Name = "Attack", Range = 1, Targets = AbilityTargets.Enemy };
        toret[ "Switch" ] = () => new SwitchAbility { Name = "Switch", Range = 1, Targets = AbilityTargets.Friendly };
        toret[ "Push" ] = () => new PushAbility { Name = "Push", Range = 1, PushRange = 1, Targets = AbilityTargets.Friendly };
        return toret;
    }

    public string AbilityName;
}