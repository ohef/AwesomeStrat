using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DisplayNumericSlider : MonoBehaviour
{
    public Slider slider;
    public Text text;
    public string unit;
    public byte decimals = 2;

    void OnEnable()
    {
        slider.onValueChanged.AddListener( ChangeValue );
        ChangeValue( slider.value );
    }
    void OnDisable()
    {
        slider.onValueChanged.RemoveAllListeners();
    }

    void ChangeValue( float value )
    {
        text.text = value.ToString( "n" + decimals ) + " " + unit;
    }
}
