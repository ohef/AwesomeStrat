using UnityEngine;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using UnityEngine.EventSystems;
using System;

public class GameTile : MonoBehaviour
{
    public Vector2Int Position;
    public int CostOfTraversal;

    public void OnDrawGizmos()
    {
        //Gizmos.DrawSphere( this.transform.position, 10f);
    }
}