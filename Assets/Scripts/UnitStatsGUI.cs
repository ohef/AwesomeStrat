using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitStatsGUI : MonoBehaviour {
    public ValueLabel HP;
    public ValueLabel Movement;
    private TextMeshProUGUI UnitName;
    private TextMeshProUGUI ClassName;

    public void Awake()
    {
        UnitName = gameObject.transform.FindChild( "UnitName" ).GetComponent<TextMeshProUGUI>();
        ClassName = gameObject.transform.FindChild( "ClassName" ).GetComponent<TextMeshProUGUI>();
    }

    public void UpdateForUnit()
    {
        Unit unit;
        if (
        BattleSystem.Instance.Map.UnitPos.TryGetValue(
        BattleSystem.Instance.Cursor.CurrentPosition,
        out unit ) )
        {
            HP.UpdateValue( unit.HP );
            Movement.UpdateValue( unit.MovementRange );
            UnitName.text = unit.Name;
            ClassName.text = unit.Class.Name;
        }
        else
        {
            UnitName.text = "";
            ClassName.text = "";
        }
    }
}
