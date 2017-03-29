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

    [MenuItem( "Window/MapEditor" )]
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
    static Vector2 scrollPosition = Vector2.zero;

    static int TeamSelectionIndex = 0;
    static int ModeSelectionIndex = 0;

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
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
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

        switch ( ModeSelectionIndex )
        {
            case 1:
                DrawTeamSelector( sceneView );
                HandleFoundTile( sceneView, PlaceUnitAt );
                break;
            case 2:
                HandleFoundTile( sceneView, PlaceTileAt );
                break;
            case 3:
                HandleFoundTile( sceneView, RemoveUnitAt );
                break;
        }
        Handles.EndGUI();
    }

    private static void DrawTeamSelector( SceneView sceneView )
    {
        GUILayout.BeginArea( new Rect( 5, 50, 150, sceneView.position.height ) );
        GUILayout.Label( "Team membership" );
        TeamSelectionIndex = GUILayout.SelectionGrid( TeamSelectionIndex, new string[] { "1", "2", "3", "4" }, 1 );
        GUILayout.EndArea();
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
            if ( JustMouseDown() )
            {
                GameTile tile = rayHitInfo.collider.GetComponent<GameTile>();
                if ( tile != null )
                    handler( tile );
            }
        }

        HandleUtility.AddDefaultControl( controlId );
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

    private static void PlaceTileAt( GameTile tile )
    {
        GameTile prefabTile = Selection.activeGameObject.GetComponent<GameTile>();
        if ( prefabTile == null )
            return;

        GameTile Tile = GameObject.Instantiate<GameTile>( prefabTile, TileLayer.transform, false );
        Tile.transform.localPosition = tile.Position.ToVector3();
        Tile.name = tile.name;
        Tile.Position = tile.Position;
        GameObject.DestroyImmediate( tile.gameObject );
        EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
    }

    private static void PlaceUnitAt( GameTile tile )
    {
        if ( GetUnit( tile ) == null )
        {
            Unit prefabUnit = Selection.activeGameObject.GetComponent<Unit>();
            if ( prefabUnit == null )
                return;
            var unit = Instantiate( prefabUnit, UnitLayer.transform, false );
            unit.transform.localPosition = tile.Position.ToVector3();
            unit.GetComponent<UnitMapHelper>().PlayerOwner = TeamSelectionIndex;
            EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
        }
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
}