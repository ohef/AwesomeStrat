using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
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
    private Dictionary<Vector2Int, TextMesh> PosToTexts = new Dictionary<Vector2Int, TextMesh>();

    public void OnEnable()
    {
        buf = new CommandBuffer();
        SceneView.onSceneGUIDelegate += OnScene;

        var obj = target as InfluenceMapPlayer;
        foreach ( Vector2Int pos in obj.Map.AllMapPositions() )
        {
            var instantiatedObj = Instantiate<TextMesh>( obj.textMeshPrefab, obj.Map.transform.Find( "Offset" ), false );
            instantiatedObj.transform.localPosition = pos.ToVector3();
            instantiatedObj.hideFlags = HideFlags.HideInInspector;
            PosToTexts[ pos ] = instantiatedObj;
        }

        CalculateInfluenceMap();
    }

    public void OnDisable()
    {
        var obj = target as InfluenceMapPlayer;
        foreach ( Vector2Int pos in obj.Map.AllMapPositions() )
        {
            DestroyImmediate( PosToTexts[ pos ].gameObject );
        }
        PosToTexts.Clear();

        SceneView.onSceneGUIDelegate -= OnScene;
    }

    public void OnScene( SceneView scene )
    {
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
                PosToTexts[ pos ].text = InfluenceMap[ pos.x, pos.y ].ToString();

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