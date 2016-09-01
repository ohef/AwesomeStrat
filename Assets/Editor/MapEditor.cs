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

    public static Unit unitPrefHolder = new Unit();

    [MenuItem("Window/MapEditor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow( typeof( MapEditor ) );
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        defaultTile = EditorGUILayout.ObjectField( defaultTile, typeof( GameTile ), true ) as GameTile;
        map = EditorGUILayout.ObjectField( map, typeof( GameMap ), true ) as GameMap;
        AddUnitInstantiator();
        EditorGUILayout.EndVertical();
    }

    void AddUnitInstantiator()
    {
        unitPrefHolder.HP = EditorGUILayout.IntField( unitPrefHolder.HP );
        unitPrefHolder.Attack = EditorGUILayout.IntField( unitPrefHolder.Attack );
        unitPrefHolder.AttackRange = EditorGUILayout.IntField( unitPrefHolder.AttackRange );
        unitPrefHolder.Defense = EditorGUILayout.IntField( unitPrefHolder.Defense );
        unitPrefHolder.Movement = EditorGUILayout.IntField( unitPrefHolder.Movement );

        int X = 0;
        int Y = 0;
        X = EditorGUILayout.IntField( X );
        Y = EditorGUILayout.IntField( Y );
        //unitPrefHolder.Position = new Assets.General.DataStructures.Vector2Int( X, Y );
    }
}
