using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsGUI : MonoBehaviour {
    public ValueLabel HP;
    public ValueLabel Attack;
    public ValueLabel Defense;
    public ValueLabel Movement;

    public void Start()
    {
        BattleSystem.Instance.Cursor.CursorMoved.AddListener( UpdateForUnit );
    }

    public void UpdateForUnit()
    {
        Unit unit;
        if(
        BattleSystem.Instance.Map.UnitGametileMap.TryGetValue(
        BattleSystem.Instance.Cursor.CurrentTile,
        out unit ) )
        {
            HP.UpdateValue( unit.HP );
            Attack.UpdateValue( unit.Attack );
            Defense.UpdateValue( unit.Defense );
            Movement.UpdateValue( unit.MovementRange );
        }
    }
}
