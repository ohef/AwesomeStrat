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
    static Vector2 scrollPosition = Vector2.zero;

    static int ItemSelectionIndex = 0;
    static int ModeSelectionIndex = 0;

    static int UnitOwner = 1;

    static string[] ToolLabels = new string[] { "Native Editing", "Place Unit", "Place Tile", "Remove Unit" };

    static GameObject UnitLayer;
    static GameObject TileLayer;
    static CommandBuffer buf = new CommandBuffer();
    static Camera SceneCamera = null;

    static Texture2D[] UnitPreviews;
    static List<MonoBehaviour> UnitPrefabs;

    static Texture2D[] TilePreviews;
    static List<MonoBehaviour> TilePrefabs;

    static MapEditorScene()
    {
        Initialize();
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public static void Initialize()
    {
        SetUpItemList( "/Prefabs/Units", ref UnitPreviews, ref UnitPrefabs );
        SetUpItemList( "/Prefabs/GameTiles", ref TilePreviews, ref TilePrefabs );
    }

    public static void SetUpItemList( string assetsPath, ref Texture2D[] ItemPreviews, ref List<MonoBehaviour> Items )
    {
        var paths = Directory.GetFileSystemEntries( Application.dataPath + assetsPath, "*.prefab" )
            .Select( path => "Assets" + path.Replace( Application.dataPath, "" ) ).ToList();

        ItemPreviews = paths
            .Select( path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path ) )
            .Select( unit => AssetPreview.GetAssetPreview( unit ) ).ToArray();

        Items = paths
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
        TileLayer = TileLayer == null ? GameObject.Find( "TileLayer" ) : TileLayer;
        Map = Map == null ? GameObject.FindGameObjectWithTag( "Map" ).GetComponent<GameMap>() : Map;

        Handles.BeginGUI();

        ModeSelectionIndex = GUILayout.SelectionGrid( ModeSelectionIndex, ToolLabels, ToolLabels.Length );
        if ( GUILayout.Button( "Reinitialize" ) )
            Initialize();

        switch ( ModeSelectionIndex )
        {
            case 1:
                DrawUnitSelectorGUI();
                HandleFoundTile( sceneView, PlaceUnitAt );
                break;
            case 2:
                DrawSelectorGUI( TilePreviews );
                HandleFoundTile( sceneView, PlaceTileAt );
                break;
            case 3:
                HandleFoundTile( sceneView, RemoveUnitAt );
                break;
        }
        Handles.EndGUI();
    }

    private static void PlaceTileAt( GameTile tile )
    {
        var obj = GameObject.Instantiate( TilePrefabs[ ItemSelectionIndex ], TileLayer.transform, false );
        var prefabTile = obj.GetComponent<GameTile>();
        prefabTile.transform.localPosition = tile.Position.ToVector3();
        prefabTile.name = tile.name;
        prefabTile.Position = tile.Position;
        GameObject.DestroyImmediate( tile.gameObject );
        EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
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
            var unit = Instantiate( UnitPrefabs[ ItemSelectionIndex ], UnitLayer.transform, false );
            unit.transform.localPosition = tile.Position.ToVector3();
            unit.GetComponent<UnitMapHelper>().PlayerOwner = UnitOwner;
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

    private static void DrawSelectorGUI( Texture2D[] previews )
    {
        scrollPosition = GUILayout.BeginScrollView( scrollPosition, EditorStyles.label );

        GUILayout.BeginVertical();

        ItemSelectionIndex = GUILayout.SelectionGrid( ItemSelectionIndex, previews, 1 );

        GUI.EndScrollView();

        GUILayout.EndVertical();
    }

    private static void DrawUnitSelectorGUI()
    {
        DrawSelectorGUI(UnitPreviews);
        GUI.BeginGroup( new Rect( new Vector2( Screen.width - 120, Screen.height - 300 ), new Vector2( 110, 300 ) ) );
        GUILayout.BeginVertical();
        GUILayout.Label( "Unit Ownership" );
        UnitOwner = GUILayout.SelectionGrid( UnitOwner, new string[] { "1", "2", "3" }, 1 );
        GUILayout.EndVertical();
        GUI.EndGroup();
    }
}