using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButtonFactory : MonoBehaviour
{
    public static AbilityButtonFactory instance;
    public Button DefaultButton;

    public void Awake()
    {
        instance = instance == null ? this : instance;
    }

    public Button Create( Ability ability )
    {
        Button toret = GameObject.Instantiate<Button>( DefaultButton );
        toret.name = ability.Name;
        toret.GetComponentInChildren<Text>().text = ability.Name;
        return toret;
    }
}