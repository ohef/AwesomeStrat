using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.SceneManagement;

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
    static GameMap Map;
    static GameTile lastHitTile;
    static List<MonoBehaviour> Units;
    static Texture2D[] UnitPreviews;
    static Vector2 scrollPosition = Vector2.zero;

    static int unitSelectionIndex = 0;

    static int toolSelectionIndex = 0;

    static string[] ToolLabels = new string[] { "Native Editing", "Place Unit", "Remove Unit", "Reinitialize" };

    static GameObject UnitLayer;

    static MapEditorScene()
    {
        Initialize();
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public static void Initialize()
    {
        var unitPaths = Directory.GetFileSystemEntries( Application.dataPath + "/Prefabs/Units", "*.prefab" )
            .Select( path => "Assets" + path.Replace( Application.dataPath, "" ) ).ToList();

        UnitPreviews = unitPaths
            .Select( path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path ) )
            .Select( unit => AssetPreview.GetAssetPreview( unit ) ).ToArray();

        Units = unitPaths
            .Select( path => AssetDatabase.LoadAssetAtPath<MonoBehaviour>( path ) ).ToList();
    }

    private static void OnSceneGUI( SceneView sceneView )
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if ( scene.name != "maptest" )
            return;

        UnitLayer = UnitLayer == null ? GameObject.Find( "UnitLayer" ) : UnitLayer;
        Map = Map == null ? GameObject.FindGameObjectWithTag( "Map" ).GetComponent<GameMap>() : Map;

        Handles.BeginGUI();
        toolSelectionIndex = GUILayout.SelectionGrid( toolSelectionIndex, ToolLabels, ToolLabels.Length );
        Handles.EndGUI();

        switch ( toolSelectionIndex )
        {
            case 1:
                DrawUnitSelectorGUI( sceneView.position );
                BeginPlacingUnit( sceneView );
                break;
            case 2:
                HandleRemoveUnitAction( sceneView );
                break;
            case 3:
                Initialize();
                break;
        }
    }

    private static bool JustMouseDown()
    {
        return Event.current.type == EventType.mouseDown &&
               Event.current.button == 0 &&
               Event.current.alt == false &&
               Event.current.shift == false &&
               Event.current.control == false;
    }

    private static void BeginPlacingUnit( SceneView scene )
    {
        int controlId = GUIUtility.GetControlID( FocusType.Passive );
        Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

        RaycastHit rayHitInfo;
        if ( Physics.Raycast( ray, out rayHitInfo ) )
        {
            DrawUnitMeshOverMap( scene, rayHitInfo );
            if ( JustMouseDown() )
            {
                PlaceUnit( rayHitInfo );
            }
        }

        HandleUtility.AddDefaultControl( controlId );
    }

    private static void PlaceUnit( RaycastHit rayHitInfo )
    {
        var unit = Instantiate( Units[ unitSelectionIndex ] );
        unit.transform.SetParent( UnitLayer.transform, false );

        var localizedPoint = Map.transform.InverseTransformPoint( rayHitInfo.point );
        unit.transform.localPosition = new Vector3(
            Mathf.Floor( localizedPoint.x ),
            0,
            Mathf.Floor( localizedPoint.z ) );

        EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
    }

    private static void DrawUnitMeshOverMap( SceneView scene, RaycastHit rayHitInfo )
    {
        Matrix4x4 rescaleHit = Matrix4x4.TRS(
            rayHitInfo.point + Vector3.Scale( Vector3.up * 0.5f, Map.transform.localScale ),
            Quaternion.identity,
            Map.transform.localScale );

        MonoBehaviour selectedUnit = Units[ unitSelectionIndex ];
        foreach ( var skinnedMeshR in selectedUnit.GetComponentsInChildren<SkinnedMeshRenderer>() )
        {
            Graphics.DrawMesh(
                skinnedMeshR.sharedMesh,
                rescaleHit,
                skinnedMeshR.sharedMaterial,
                0,
                scene.camera );
        }

        foreach ( var meshfilter in selectedUnit.GetComponentsInChildren<MeshFilter>() )
        {
            foreach ( var meshrenderer in selectedUnit.GetComponentsInChildren<MeshRenderer>() )
            {
                Graphics.DrawMesh( meshfilter.sharedMesh,
                    rescaleHit, meshrenderer.sharedMaterial,
                    0, scene.camera );
            }
        }

        //Graphics.DrawMesh(
        //    Units[ unitSelectionIndex ].GetComponent<MeshFilter>().sharedMesh,
        //    rescaleHit,
        //    Units[ unitSelectionIndex ].GetComponent<MeshRenderer>().sharedMaterial,
        //    0,
        //    scene.camera );

        HandleUtility.Repaint();
    }

    private static void HandleRemoveUnitAction( SceneView scene )
    {
        int controlId = GUIUtility.GetControlID( FocusType.Passive );
        Vector2 mousePosition = new Vector2( Event.current.mousePosition.x, Event.current.mousePosition.y );
        Ray ray = HandleUtility.GUIPointToWorldRay( mousePosition );

        RaycastHit rayHitInfo;
        if ( Physics.Raycast( ray, out rayHitInfo ) && JustMouseDown() )
        {
            Vector3 hitUnitTile = new Vector3(
                    Mathf.Floor( rayHitInfo.point.x / Map.transform.localScale.x ),
                    0,
                    Mathf.Floor( rayHitInfo.point.z / Map.transform.localScale.z ) );

            MapUnit hitUnit =
            FindObjectsOfType<MapUnit>().FirstOrDefault( unit =>
            Mathf.Floor( unit.transform.localPosition.x ) == hitUnitTile.x &&
            Mathf.Floor( unit.transform.localPosition.z ) == hitUnitTile.z
            );

            if ( hitUnit != null )
            {
                GameObject.DestroyImmediate( hitUnit.gameObject );
                EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
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