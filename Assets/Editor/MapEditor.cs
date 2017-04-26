using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.Rendering;
using Assets.General.UnityExtensions;
using Assets.General.DataStructures;
using System.Linq;

[ExecuteInEditMode]
public class MapEditor : EditorWindow
{
    public static GameMap map = null;
    public static GameTile InitTile;
    public static int Width = 0;
    public static int Height = 0;
    public static Vector2IntExt.Axis InstantiationAxis;

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
        InitTile = EditorGUILayout.ObjectField( InitTile, typeof( GameTile ), true ) as GameTile;

        EditorGUILayout.BeginHorizontal();
        Width = EditorGUILayout.IntField( "Width", Width );
        Height = EditorGUILayout.IntField( "Height", Height );
        InstantiationAxis = ( Vector2IntExt.Axis )EditorGUILayout.EnumPopup( InstantiationAxis );
        EditorGUILayout.EndHorizontal();

        if ( GUILayout.Button( "Create Map" ) )
        {
            map.InitializeMap( Width, Height, InitTile, true, Vector2IntExt.Axis.Z );
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

    //static GameObject UnitLayer;
    static GameObject TileLayer;
    static CommandBuffer buf = new CommandBuffer();
    static Camera SceneCamera = null;

    static Texture2D[] UnitPreviews;
    static List<MonoBehaviour> UnitPrefabs;

    static Texture2D[] TilePreviews;
    static List<MonoBehaviour> TilePrefabs;
    static TurnController[] TurnControllers;

    static MapEditorScene()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private static void LoadPlayers()
    {
        TurnControllers = GameObject.FindObjectsOfType<TurnController>();
        Map = GameObject.FindGameObjectWithTag( "Map" ).GetComponent<GameMap>();
        TileLayer = GameObject.Find( "TileLayer" );
    }

    private static void OnSceneGUI( SceneView sceneView )
    {
        if ( SceneCamera == null )
        {
            SceneCamera = sceneView.camera;
            SceneCamera.AddCommandBuffer( CameraEvent.AfterSkybox, buf );
        }

        TileLayer = TileLayer ?? GameObject.Find( "TileLayer" );
        Map = Map ?? GameObject.FindGameObjectWithTag( "Map" ).GetComponent<GameMap>();
        TurnControllers = TurnControllers ?? GameObject.FindObjectsOfType<TurnController>();

        Handles.BeginGUI();
        ModeSelectionIndex = GUILayout.SelectionGrid( ModeSelectionIndex, ToolLabels, ToolLabels.Length );
        if ( GUILayout.Button( "ReloadPlayers" ) )
            LoadPlayers();

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
        TeamSelectionIndex = GUILayout.SelectionGrid( 
            TeamSelectionIndex, 
            TurnControllers.Select( x => x.PlayerNo.ToString() ).ToArray(), 1 );
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

    private static void HandleFoundTile( SceneView scene, Action<Vector2Int> handler )
    {
        int controlId = GUIUtility.GetControlID( FocusType.Passive );
        Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

        RaycastHit rayHitInfo;
        if ( Physics.Raycast( ray, out rayHitInfo ) )
        {
            if ( JustMouseDown() )
            {
                GameTile tile = rayHitInfo.collider.GetComponent<GameTile>();
                var pos =
                new Vector2Int(
                    ( int )Mathf.Floor( rayHitInfo.transform.localPosition.x ),
                    ( int )Mathf.Floor( rayHitInfo.transform.localPosition.y ) );
                if ( tile != null )
                    handler( pos );
            }
        }

        HandleUtility.AddDefaultControl( controlId );
    }

    private static void PlaceTileAt( Vector2Int pos )
    {
        GameTile prefabTile = Selection.activeGameObject.GetComponent<GameTile>();
        if ( prefabTile == null )
            return;

        GameTile Tile = GameObject.Instantiate<GameTile>( prefabTile, TileLayer.transform, false );
        Tile.transform.localPosition = MapPositionTranslator.GetTransform( pos );
        var TileAtPos = Map.TilePos[ pos ];
        Tile.name = TileAtPos.name;
        GameObject.DestroyImmediate( TileAtPos.gameObject );
        Map.TilePos.Add( Tile, pos );
        EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
    }

    private static void PlaceUnitAt( Vector2Int pos )
    {
        Unit unitAtPos;
        Map.UnitPos.TryGetValue( pos, out unitAtPos );
        if ( unitAtPos == null )
        {
            Unit prefabUnit = Selection.activeGameObject.GetComponent<Unit>();
            if ( prefabUnit == null )
                return;
            var unit = Instantiate( prefabUnit, TurnControllers[ TeamSelectionIndex ].transform, false );
            unit.transform.localPosition = MapPositionTranslator.GetTransform( pos );
            Map.UnitPos.Add( unit, pos );

            unit.RegisterTurnController( TurnControllers[ TeamSelectionIndex ] );
            unit.UnitChanged.Invoke();

            EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
        }
    }

    private static void RemoveUnitAt( Vector2Int pos )
    {
        Unit unitAtPos = Map.UnitPos[ pos ];
        if ( unitAtPos == null )
            return;
        else
        {
            Map.UnitPos.Remove( pos );
            GameObject.DestroyImmediate( unitAtPos.gameObject );
            EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
        }
    }
}