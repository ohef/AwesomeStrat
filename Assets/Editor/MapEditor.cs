using UnityEngine;
using UnityEditor;
using Assets.Map;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using System;

[ExecuteInEditMode]
public class MapEditor : EditorWindow
{
    public static GameMap map = null;
    public int Width = 0;
    public int Height = 0;

    private static GameObject CameraToFocus;
    private static GameObject CursorFocusedOn;

    [MenuItem("Window/MapEditor")]
    public static void Init()
    {
        map = GameObject.FindObjectOfType<GameMap>();
        EditorWindow.GetWindow<MapEditor>();

        CameraToFocus = GameObject.Find( "Main Camera" );
        CursorFocusedOn = GameObject.Find( "Cursor" );
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
            EditorSceneManager.MarkAllScenesDirty();
        }

        if ( GUILayout.Button( "Center Camera on Cursor" ) )
        {
            CameraToFocus.transform.LookAt( CursorFocusedOn.transform );
        }

        EditorGUILayout.EndVertical();
    }
}

[InitializeOnLoad]
public class MapEditorScene : Editor
{
    static GameMap map;
    static GameTile lastHitTile;
    static List<MonoBehaviour> Units;
    static Texture2D[] UnitPreviews;
    static Vector2 scrollPosition = Vector2.zero;

    static int unitSelectionIndex = 0;

    static int toolSelectionIndex = 0;

    static string[] ToolLabels = new string[] { "Native Editing", "Place Unit", "Remove Unit" };

    static MapEditorScene()
    {
        var unitPaths = Directory.GetFileSystemEntries( Application.dataPath + "/Prefabs/Units", "*.prefab" )
            .Select( path => "Assets" + path.Replace( Application.dataPath, "" ) ).ToList();

        UnitPreviews = unitPaths
            .Select( path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path ) )
            .Select( unit => AssetPreview.GetAssetPreview( unit ) ).ToArray();

        Units = unitPaths
            .Select( path => AssetDatabase.LoadAssetAtPath<MonoBehaviour>( path ) ).ToList();

        map = GameObject.FindObjectOfType<GameMap>();

        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private static void OnSceneGUI( SceneView scene )
    {
        Handles.BeginGUI();
        toolSelectionIndex = GUILayout.SelectionGrid( toolSelectionIndex, ToolLabels, ToolLabels.Length );
        Handles.EndGUI();

        if ( toolSelectionIndex == 1 ) //Place Unit
        {
            DrawUnitSelectorGUI( scene.position );
            HandlePlaceUnitAction( scene );
        }
        else if ( toolSelectionIndex == 2 )
        {
            HandleRemoveUnitAction( scene );
        }
    }

    private static void HandlePlaceUnitAction( SceneView scene )
    {
        int controlId = GUIUtility.GetControlID( FocusType.Passive );
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

                EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
            }
        }

        HandleUtility.AddDefaultControl( controlId );
    }

    private static void HandleRemoveUnitAction( SceneView scene )
    {
        int controlId = GUIUtility.GetControlID( FocusType.Passive );
        Vector2 mousePosition = new Vector2( Event.current.mousePosition.x, Event.current.mousePosition.y );
        Ray ray = HandleUtility.GUIPointToWorldRay( mousePosition );

        RaycastHit rayHitInfo;
        if ( Physics.Raycast( ray, out rayHitInfo ) )
        {
            if ( Event.current.type == EventType.mouseDown &&
                Event.current.button == 0 &&
                Event.current.alt == false &&
                Event.current.shift == false &&
                Event.current.control == false )
            {
                Vector3 hitUnitTile = new Vector3(
                    Mathf.Floor( rayHitInfo.point.x ),
                    0,
                    Mathf.Floor( rayHitInfo.point.z ) );

                Unit hitUnit = 
                FindObjectsOfType<Unit>().FirstOrDefault( unit =>
                Mathf.Floor( unit.transform.position.x ) == hitUnitTile.x &&
                Mathf.Floor( unit.transform.position.z ) == hitUnitTile.z
                );

                if ( hitUnit != null )
                {
                    GameObject.DestroyImmediate( hitUnit.gameObject );
                    EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
                }
            }
        }

        HandleUtility.AddDefaultControl( controlId );
    }

    private static void DrawUnitSelectorGUI( Rect position )
    {
        Handles.BeginGUI();

        scrollPosition = GUILayout.BeginScrollView( scrollPosition, EditorStyles.label );

        GUILayout.BeginVertical();

        unitSelectionIndex = GUILayout.SelectionGrid( unitSelectionIndex, UnitPreviews, 1 );

        GUILayout.EndVertical();

        GUI.EndScrollView();

        Handles.EndGUI();
    }
}