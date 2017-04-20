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
            Defense.UpdateValue( unit.Defense );
            Movement.UpdateValue( unit.MovementRange );
            unit.Abilities.ForEach(
                ability => 
                AbilityButtonFactory.instance.CreateWithoutFunctionality( ability )
                .transform.SetParent( Menu.transform ) );
        }
    }
}
