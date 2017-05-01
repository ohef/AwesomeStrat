using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Assets.General.DataStructures;

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
        Vector2Int currentTile = BattleSystem.Instance.Cursor.CurrentPosition;
        Unit unitAtTile;
        if ( BattleSystem.Instance.Map.UnitPos.TryGetValue( currentTile, out unitAtTile ) == true )
        {
            HP.text = unitAtTile.HP.ToString();
            Move.text = unitAtTile.MovementRange.ToString();
            //Defense.text = unitAtTile.Defense.ToString();
        }
    }
}
