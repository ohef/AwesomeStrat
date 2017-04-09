using Assets.General.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor( typeof( InfluenceMapPlayer ) )]
public class InfluenceMapPlayerEditor : Editor
{
    private float[,] InfluenceMap;
    private CommandBuffer buf;

    public void OnEnable()
    {
        buf = new CommandBuffer();
        SceneView.onSceneGUIDelegate += OnScene;
    }

    public void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnScene;
    }

    public void OnScene( SceneView scene )
    {
        CalculateInfluenceMap();
        Graphics.ExecuteCommandBuffer( buf );
    }

    public void CalculateInfluenceMap()
    {
        InfluenceMapPlayer obj = target as InfluenceMapPlayer;
        InfluenceMap = obj.CalculateInfluenceMap();

        if ( obj.InfluenceMaterial != null && obj.Map != null )
        {
            GameMap map = GameObject.FindObjectOfType<GameMap>();
            foreach ( Vector2Int pos in map.AllMapPositions())
            {
                Material mat = Material.Instantiate( obj.InfluenceMaterial );
                mat.SetFloat( "_Alpha", InfluenceMap[ pos.x, pos.y ] );
                GameTile tile = map.TilePos[ pos ];
                buf.DrawMesh(
                    tile.GetComponent<MeshFilter>().sharedMesh,
                    tile.transform.localToWorldMatrix,
                    mat, 0 );
            }
        }
    }
}
