using UnityEngine;
using System.Collections;
using Assets.Map;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using UnityEngine.EventSystems;
using System;

[ExecuteInEditMode]
public class GameTile : MonoBehaviour
{

    public Tile tileData;

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere( this.transform.position, 0.25f );
    }

    // Use this for initialization
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        transform.position = tileData.Position.ToVector3( Vector2IntExtensions.Axis.Y, 0.0f ) - 0.5f * ( Vector3.left + Vector3.back );
        if ( tileData.UnitOccupying != null )
        {
        }
    }
}
