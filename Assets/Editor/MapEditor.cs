using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets.Map;

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
