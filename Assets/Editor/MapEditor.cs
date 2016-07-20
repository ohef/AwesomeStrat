using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets.Map;

[ExecuteInEditMode]
public class MapEditor : EditorWindow
{
    public GameMap map = null;

    [MenuItem("Window/MyWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow( typeof( MapEditor ) );
    }

    void OnGUI()
    {

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField( "MapCreator and some other stuff" );
        map = EditorGUILayout.ObjectField( map, typeof( GameMap ), true ) as GameMap;
        if ( map.MapInternal != null )
        {
            EditorGUILayout.LabelField( string.Format( "Map Size: {0}", map.MapInternal.MapSize ) );
        }
        EditorGUILayout.EndVertical();
    }
}
