using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo : MonoBehaviour {
    public ValueLabel TileCost;

    public void UpdateInfo()
    {
        TileCost.UpdateValue( BattleSystem.Instance.Cursor.CurrentTile.CostOfTraversal );
    }
}
