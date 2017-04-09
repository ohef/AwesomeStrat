using Assets.General.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfluenceMapPlayer : MonoBehaviour
{
    public GameMap Map;
    public float[,] InfluenceMap;
    public Material InfluenceMaterial;
    public int Player;

    public float[,] CalculateInfluenceMap()
    {
        float[,] influenceMap = new float[ Map.Width, Map.Height ];
        foreach ( Vector2Int pos in Map.AllMapPositions() )
        {
            IEnumerable<Unit> units = GameObject.FindObjectsOfType<Unit>();
            int unitCount = units.Count();
            influenceMap[ pos.x, pos.y ] =
                units.Aggregate<Unit, float>( 0,
                ( accumulate, unit ) => CalculateContribution( unit, pos ) + accumulate ) / unitCount;
        }
        return influenceMap;
    }

    private float CalculateContribution( Unit unit, Vector2Int pos )
    {
        float distance = pos.ManhattanDistance( Map.UnitPos[ unit ] );
        return distance == 0 ? 1 : 1 / distance;
    }

    public void OnDrawGizmosSelected()
    {
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
