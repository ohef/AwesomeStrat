using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets.Map;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;

[ExecuteInEditMode]
public class MapEditor : EditorWindow
{
    public static GameMap map = null;
    public int Width = 0;
    public int Height = 0;

    [MenuItem("Window/MapEditor")]
    public static void Init()
    {
        map = GameObject.FindObjectOfType<GameMap>();
        EditorWindow.GetWindow<MapEditor>();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        map = EditorGUILayout.ObjectField( map, typeof( GameMap ), true ) as GameMap;

        EditorGUILayout.BeginHorizontal();
        Width = EditorGUILayout.IntField( "Width", Width );
        Height = EditorGUILayout.IntField( "Height", Height );
        EditorGUILayout.EndHorizontal();

        if ( GUILayout.Button( "Create Map" ) )
        {
            map.ReInitializeMap( Width, Height );
        } 

        EditorGUILayout.EndVertical();
    }
}

[InitializeOnLoad]
public class MapEditorScene : Editor
{
    static GameMap map;
    static GameTile lastHitTile;
    static MapEditorScene()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public static void OnSceneGUI( SceneView scene )
    {
        map = GameObject.FindObjectOfType<GameMap>();
        Vector2 mousePosition = new Vector2( Event.current.mousePosition.x, Event.current.mousePosition.y );
        Ray ray = HandleUtility.GUIPointToWorldRay( mousePosition );
        RaycastHit rayHitInfo;
        if( Physics.Raycast( ray, out rayHitInfo ) )
        {
            Vector2Int tileHit = new Vector2Int( ( int )Mathf.Floor( rayHitInfo.point.x ), ( int )Mathf.Floor( rayHitInfo.point.z ) );
            GameTile gameTileHit = map[ tileHit ];
            if( lastHitTile == gameTileHit )
            {
                gameTileHit.GetComponent<Material>().color = new Color( 0.5f, 0.25f, 0.25f, 1.0f );
                lastHitTile.GetComponent<Material>().color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
                lastHitTile = gameTileHit;
                Handles.CubeCap( 10, gameTileHit.Position.ToVector3( Vector2IntExtensions.Axis.Y ), Quaternion.identity, 10f );
            }
        }
    }
}