using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets.Map;

[ExecuteInEditMode]
public class MapEditor : EditorWindow
{
    public GameMap map = null;
    public int width= 0;
    public int height= 0;
    public GameTile defaultTile;

    [MenuItem("Window/MyWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow( typeof( MapEditor ) );
    }

    void OnGUI()
    {

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField( "It's a map maker, whaddaya want me to do" );

        EditorGUILayout.LabelField( "Width: " );
        width = EditorGUILayout.IntField( width );

        EditorGUILayout.LabelField( "Height: " );
        height = EditorGUILayout.IntField( height );

        defaultTile = EditorGUILayout.ObjectField( defaultTile, typeof( GameTile ), true ) as GameTile;
        EditorGUILayout.EndVertical();
    }
}
