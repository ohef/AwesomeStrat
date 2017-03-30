using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueLabel : MonoBehaviour {
    private Text TextLabel;
    private Text TextValue;

    public void Awake()
    {
        TextLabel = this.transform.Find( "Label" ).GetComponent<Text>();
        TextValue = this.transform.Find( "Value" ).GetComponent<Text>();
    }

    public void UpdateValue( int value ) { TextValue.text = value.ToString(); }
}
