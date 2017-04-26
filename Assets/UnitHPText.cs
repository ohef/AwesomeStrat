using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitHPText : MonoBehaviour {

    private TextMeshPro text;
    private Unit unit;
	// Use this for initialization
	void Start () {
        unit = transform.parent.GetComponent<Unit>();
        text = GetComponent<TextMeshPro>();
	}

    public void UpdateForHP()
    {
        text.text = unit.HP.ToString();
    }
}
