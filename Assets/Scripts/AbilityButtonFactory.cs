﻿using System;
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

    public Button Create( IAbility ability )
    {
        Button toret = GameObject.Instantiate<Button>( DefaultButton );
        toret.name = ability.Name;
        toret.GetComponentInChildren<Text>().text = ability.Name;
        return toret;
    }

    public GameObject CreateWithoutFunctionality( IAbility ability )
    {
        Button toDestroy = Create( ability );
        GameObject toret = toDestroy.gameObject;
        GameObject.Destroy( toDestroy );
        return toret;
    }
}