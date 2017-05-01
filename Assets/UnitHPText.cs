using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;
using System;

[ExecuteInEditMode]
public class UnitHPText : MonoBehaviour {

    private TextMeshPro text;
	// Use this for initialization
	void Awake () {
        Unit unit = transform.parent.GetComponent<Unit>();
        text = GetComponent<TextMeshPro>();
        PropertyInfo prop = typeof( Unit ).GetProperty( this.name );
        Action<Unit> toWire = delegate ( Unit eventUnit ) { text.text = prop.GetValue( eventUnit, null ).ToString(); };
        unit.UnitChanged += toWire;
    }

    //public void UpdateForHP()
    //{
    //    text.text = unit.HP.ToString();
    //}
}