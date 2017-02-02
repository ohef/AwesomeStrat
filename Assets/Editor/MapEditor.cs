using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets.Map;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System.Linq;
using System.Collections.Generic;
using System.IO;

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
    static Vector2 scrollPosition = Vector2.zero;
    static List<MonoBehaviour> Units;
    static Texture2D[] UnitPreviews;
    //static List<Object> Units;
    static int unitSelectionIndex = 0;

    static MapEditorScene()
    {
        var unitPaths = Directory.GetFileSystemEntries( Application.dataPath + "/Prefabs/Units", "*.prefab" )
            .Select( path => "Assets" + path.Replace( Application.dataPath, "" ) ).ToList();

        UnitPreviews = unitPaths
            .Select( path => AssetDatabase.LoadAssetAtPath<Object>( path ) )
            .Select( unit => AssetPreview.GetAssetPreview( unit ) ).ToArray();

        Units = unitPaths
            .Select( path => AssetDatabase.LoadAssetAtPath<MonoBehaviour>( path ) ).ToList();

        map = GameObject.FindObjectOfType<GameMap>();

        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public static void OnSceneGUI( SceneView scene )
    {
        Handles.BeginGUI();

        scrollPosition = GUILayout.BeginScrollView( scrollPosition, EditorStyles.label );

        GUILayout.BeginVertical();

        unitSelectionIndex = GUILayout.SelectionGrid( unitSelectionIndex, UnitPreviews, 1 );

        GUILayout.EndVertical();

        GUI.EndScrollView();

        Handles.EndGUI();

        Vector2 mousePosition = new Vector2( Event.current.mousePosition.x, Event.current.mousePosition.y );
        Ray ray = HandleUtility.GUIPointToWorldRay( mousePosition );

        RaycastHit rayHitInfo;
        if ( Physics.Raycast( ray, out rayHitInfo ) )
        {

            Graphics.DrawMesh(
                Units[ unitSelectionIndex ].GetComponent<MeshFilter>().sharedMesh,
                rayHitInfo.point,
                Quaternion.identity,
                Units[ unitSelectionIndex ].GetComponent<MeshRenderer>().sharedMaterial,
                0,
                scene.camera );

            HandleUtility.Repaint();
            //scene.Repaint();

            if ( Event.current.type == EventType.mouseDown &&
                Event.current.button == 0 &&
                Event.current.alt == false &&
                Event.current.shift == false &&
                Event.current.control == false )
            {
                var unit = Instantiate( Units[ unitSelectionIndex ] );
                unit.transform.SetParent( map.ObjectOffset );
                unit.transform.localPosition = new Vector3(
                    ( int )Mathf.Floor( rayHitInfo.point.x ),
                    0,
                    ( int )Mathf.Floor( rayHitInfo.point.z ) );
            }
        }
    }
}