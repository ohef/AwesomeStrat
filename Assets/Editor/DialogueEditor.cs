using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class DialogueEditor : EditorWindow {
    static DialogueScene CurrentSceneEditing;
    static ReorderableList EntriesList;
    static string SaveName = "";

    [MenuItem( "Window/DialogueEditor" )]
    public static void Init()
    {
        EditorWindow.GetWindow<DialogueEditor>();
    }

    private void CreateNewDialogue()
    {
        CurrentSceneEditing = CreateInstance<DialogueScene>();
        CurrentSceneEditing.Entries = new List<ActorEntry>();
        EntriesList = new ReorderableList( CurrentSceneEditing.Entries, typeof( ActorEntry ) );

        EntriesList.drawHeaderCallback += DrawHeader;
        EntriesList.drawElementCallback += DrawElement;

        EntriesList.onAddCallback += AddItem;
        EntriesList.onRemoveCallback += RemoveItem;
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField( "It's supposed to be useful man fuck" );

        if ( CurrentSceneEditing == null )
        {
            EditorGUILayout.LabelField( "No Dialogue Scene Loaded, please create a new scene" );
        }
        else
        {
            EntriesList.DoLayoutList();
        }

        if ( GUILayout.Button( "Create New Scene" ) )
        {
            CreateNewDialogue();
        }

        SaveName = GUILayout.TextField( SaveName );
        if ( GUILayout.Button( "Save Scene" ) && CurrentSceneEditing != null )
        {
            AssetDatabase.CreateAsset( CurrentSceneEditing, SaveName );
            AssetDatabase.SaveAssets();
        }
    }

    private void OnDisable()
    {
        EntriesList.drawHeaderCallback -= DrawHeader;
        EntriesList.drawElementCallback -= DrawElement;

        EntriesList.onAddCallback -= AddItem;
        EntriesList.onRemoveCallback -= RemoveItem;
    }

    private void DrawHeader( Rect rect )
    {
        GUI.Label( rect, "Scene" );
    }

    private void DrawElement( Rect rect, int index, bool active, bool focused )
    {
        ActorEntry item = EntriesList.list[ index ] as ActorEntry;

        //EditorGUI.BeginChangeCheck();

        EditorGUI.LabelField( rect, item.Speaker, item.Dialogue );

        if ( active )
        {
            item.Speaker = EditorGUILayout.TextField( "Speaker", item.Speaker );
            item.Dialogue = EditorGUILayout.TextField( "Dialogue", item.Dialogue );
            item.Actions = ( EntryActions )EditorGUILayout.EnumPopup( "Actions", item.Actions );
            item.ActorSprite = EditorGUILayout.ObjectField( "ActorSprite", item.ActorSprite, typeof( Sprite ), false ) as Sprite;
        }

        //item.boolValue = EditorGUI.Toggle( new Rect( rect.x, rect.y, 18, rect.height ), item.boolValue );
        //item.stringvalue = EditorGUI.TextField( new Rect( rect.x + 18, rect.y, rect.width - 18, rect.height ), item.stringvalue );
        //if ( EditorGUI.EndChangeCheck() )
        //{
        //    EditorUtility.SetDirty( target );
        //}

        // If you are using a custom PropertyDrawer, this is probably better
        // EditorGUI.PropertyField(rect, serializedObject.FindProperty("list").GetArrayElementAtIndex(index));
        // Although it is probably smart to cach the list as a private variable ;)
    }

    private void AddItem( ReorderableList list )
    {
        EntriesList.list.Add( new ActorEntry() );

        //EditorUtility.SetDirty( target );
    }

    private void RemoveItem( ReorderableList list )
    {
        EntriesList.list.RemoveAt( list.index );

        //EditorUtility.SetDirty( target );
    }
}
