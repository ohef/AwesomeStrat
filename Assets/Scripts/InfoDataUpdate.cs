using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

/// <summary>
/// TODO: Setting up the gui like this kinda sucks; is there a better way?
/// </summary>
public class InfoDataUpdate : MonoBehaviour {

    private Text HP;
    private Text Move;
    private Text Attack;
    private Text Defense;

    public void Awake()
    {
        //Assumes that you're selecting the gameobject that is the parent of these gameobject specific GUI stuff
        Func<string,Text> getLabel = label => gameObject.transform.Find( label ).GetComponent<Text>();
        HP = getLabel( "HP" );
        Move = getLabel( "Move" );
        Attack = getLabel( "Attack" );
        Defense = getLabel( "Defense" );
    }

    public void UpdateInfo() {
        var currentTile = BattleSystem.Instance.Cursor.CurrentTile;
        Unit unitAtTile;
        if ( BattleSystem.Instance.Map.UnitGametileMap.TryGetValue( currentTile, out unitAtTile ) == true )
        {
            HP.text = unitAtTile.HP.ToString();
            Move.text = unitAtTile.MovementRange.ToString();
            Attack.text = unitAtTile.Attack.ToString();
            Defense.text = unitAtTile.Defense.ToString();
        }
    }
}
