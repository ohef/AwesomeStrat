using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent( typeof( MapUnit ) )]
public class HookLocalGUIToUnit : MonoBehaviour {

    private MapUnit Unit;
    public IntegerBar Bar; 
    public Text HPText; 

    void Awake()
    {
        Unit = this.GetComponent<MapUnit>();
    }

    public void UpdateRelatedViews()
    {
        Bar.UpdateBar( Unit.HP, Unit.MaxHP );
        HPText.text = Unit.HP.ToString();
    }
}
