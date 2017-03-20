using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Assets.General.UnityExtensions;

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
    static CommandBuffer buf = new CommandBuffer();
    static Camera SceneCamera = null;

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
        if ( SceneCamera == null )
        {
            SceneCamera = sceneView.camera;
            SceneCamera.AddCommandBuffer( CameraEvent.AfterSkybox, buf );
        }

        UnitLayer = UnitLayer == null ? GameObject.Find( "UnitLayer" ) : UnitLayer;
        Map = Map == null ? GameObject.FindGameObjectWithTag( "Map" ).GetComponent<GameMap>() : Map;

        Handles.BeginGUI();
        toolSelectionIndex = GUILayout.SelectionGrid( toolSelectionIndex, ToolLabels, ToolLabels.Length );
        Handles.EndGUI();

        switch ( toolSelectionIndex )
        {
            case 1:
                DrawUnitSelectorGUI( sceneView.position );
                HandleFoundTile( sceneView, PlaceUnitAt );
                break;
            case 2:
                HandleFoundTile( sceneView, RemoveUnitAt );
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

    private static void HandleFoundTile( SceneView scene, Action<GameTile> handler )
    {
        int controlId = GUIUtility.GetControlID( FocusType.Passive );
        Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

        RaycastHit rayHitInfo;
        if ( Physics.Raycast( ray, out rayHitInfo ) )
        {
            //DrawUnitMeshOverMap( scene, rayHitInfo );
            if ( JustMouseDown() )
            {
                GameTile tile = rayHitInfo.collider.GetComponent<GameTile>();
                if ( tile != null )
                    handler( tile );
            }
        }

        HandleUtility.AddDefaultControl( controlId );
    }

    private static void DrawUnitMeshOverMap( SceneView scene, RaycastHit rayHitInfo )
    {
        //MonoBehaviour selectedUnit = Units[ unitSelectionIndex ];

        ////buf.Clear();
        ////foreach ( var ren in selectedUnit.GetComponentsInChildren<Renderer>() )
        ////{
        ////    buf.DrawRenderer( ren, ren.sharedMaterial );
        ////}

        //Vector3 newp = UnitLayer.transform.worldToLocalMatrix * rayHitInfo.point;
        //Matrix4x4 rescaleHit = Matrix4x4.TRS(
        //    rayHitInfo.point,
        //    Quaternion.identity,
        //    Map.transform.localScale );

        //MonoBehaviour selectedUnit = Units[ unitSelectionIndex ];
        //foreach ( var skinnedMeshR in selectedUnit.GetComponentsInChildren<SkinnedMeshRenderer>() )
        //{
        //    Graphics.DrawMesh(
        //        skinnedMeshR.sharedMesh,
        //        rescaleHit,
        //        skinnedMeshR.sharedMaterial,
        //        0,
        //        scene.camera );
        //}

        //foreach ( var meshfilter in selectedUnit.GetComponentsInChildren<MeshFilter>() )
        //{
        //    foreach ( var meshrenderer in selectedUnit.GetComponentsInChildren<MeshRenderer>() )
        //    {
        //        //buf.DrawMesh( meshfilter.sharedMesh,
        //        //    rescaleHit, meshrenderer.sharedMaterial,
        //        //    0 );
        //        Graphics.DrawMesh( meshfilter.sharedMesh,
        //            rescaleHit, meshrenderer.sharedMaterial,
        //            0, scene.camera );
        //    }
        //}

        //Graphics.DrawMesh(
        //    Units[ unitSelectionIndex ].GetComponent<MeshFilter>().sharedMesh,
        //    rescaleHit,
        //    Units[ unitSelectionIndex ].GetComponent<MeshRenderer>().sharedMaterial,
        //    0,
        //    scene.camera );

        //HandleUtility.Repaint();
    }

    private static void PlaceUnitAt( GameTile tile )
    {
        if ( GetUnit( tile ) == null )
        {
            var unit = Instantiate( Units[ unitSelectionIndex ]);
            unit.transform.SetParent( UnitLayer.transform, false );
            unit.transform.localPosition = tile.Position.ToVector3();
            EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
        }
    }

    private static Unit GetUnit( GameTile tile )
    {
        Unit hitUnit =
        FindObjectsOfType<Unit>().FirstOrDefault( unit =>
        Mathf.Floor( unit.transform.localPosition.x ) == tile.Position.x &&
        Mathf.Floor( unit.transform.localPosition.z ) == tile.Position.y
        );

        return hitUnit;
    }

    private static void RemoveUnitAt( GameTile tile )
    {
        Unit hitUnit = GetUnit( tile );
        if ( hitUnit == null )
            return;
        else
        {
            GameObject.DestroyImmediate( hitUnit.gameObject );
            EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
        }
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