using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Unit ) )]
public class UnitMapHelper : MonoBehaviour {
    public int PlayerOwner;

    public void Start()
    {
        var unit = this.GetComponent<Unit>();
        var unitPosition = unit.transform.localPosition;
        BattleSystem.Instance.Map.UnitGametileMap.Add( unit,
        BattleSystem.Instance.Map[ ( int )unitPosition.x, ( int )unitPosition.z ] );
    }
}
