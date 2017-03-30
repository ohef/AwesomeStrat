using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Unit ) )]
public class UnitMapHelper : MonoBehaviour {
    public int PlayerOwner;

    public void Start()
    {
        BattleSystem.Instance.Map.AddUnit( this.GetComponent<Unit>() );
    }
}
