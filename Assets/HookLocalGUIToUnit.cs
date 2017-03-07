using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent( typeof( Unit ) )]
public class HookLocalGUIToUnit : MonoBehaviour {

    private Unit Unit;
    public IntegerBar Bar; 
    public Text HPText; 

    void Awake()
    {
        Unit = this.GetComponent<Unit>();
    }

    public void UpdateRelatedViews()
    {
        Bar.UpdateBar( Unit.HP, Unit.MaxHP );
        HPText.text = Unit.HP.ToString();
    }
}
