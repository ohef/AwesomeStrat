using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfoPanel : MonoBehaviour {

    public ValueLabel HP;
    public ValueLabel Defense;
    public ValueLabel Movement;
    public CommandMenu Menu;

    public void UpdateForUnit()
    {
        foreach ( Transform child in Menu.transform )
            Object.Destroy( child.gameObject );

        Unit unit;
        if (
        BattleSystem.Instance.Map.UnitPos.TryGetValue(
        BattleSystem.Instance.Cursor.CurrentPosition,
        out unit ) )
        {
            HP.UpdateValue( unit.HP );
            Movement.UpdateValue( unit.MovementRange );
            foreach ( var ability in unit.Abilities )
            {
                AbilityButtonFactory.instance.CreateWithoutFunctionality( ability )
                .transform.SetParent( Menu.transform );
            }
        }
    }
}
