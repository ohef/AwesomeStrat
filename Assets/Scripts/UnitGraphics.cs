using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent( typeof( Unit ) )]
public class UnitGraphics : MonoBehaviour {

    private Unit Unit;
    public MeshRenderer UnitIndicator;
    public IntegerBar Bar; 
    public Text HPText;
    public GameObject Model;

    void Awake()
    {
        Unit = this.GetComponent<Unit>();
        Unit.UnitChanged.AddListener( UpdateRelatedViews );
    }

    public void UpdateRelatedViews()
    {
        Bar.UpdateBar( Unit.HP, Unit.MaxHP );
        HPText.text = Unit.HP.ToString();
    }
}
