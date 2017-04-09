using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsGUI : MonoBehaviour {
    public ValueLabel HP;
    public ValueLabel Attack;
    public ValueLabel Defense;
    public ValueLabel Movement;

    public void UpdateForUnit()
    {
        Unit unit;
        if(
        BattleSystem.Instance.Map.UnitPos.TryGetValue(
        BattleSystem.Instance.Cursor.CurrentPosition,
        out unit ) )
        {
            HP.UpdateValue( unit.HP );
            Defense.UpdateValue( unit.Defense );
            Movement.UpdateValue( unit.MovementRange );
        }
    }
}
