using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueSystem : MonoBehaviour, ISubmitHandler
{
    public DialogueScene ToUse;
    public LinearStateMachine<ActorEntry> Session;
    private TextMeshProUGUI SpeakerText;
    private TextMeshProUGUI DialogueText;

    public void Awake()
    {
        EventSystem.current.SetSelectedGameObject( this.gameObject );
        DialogueText = transform.FindChild( "Dialogue" ).FindChild( "Text" ).GetComponent<TextMeshProUGUI>();
        SpeakerText = transform.FindChild( "Speaker" ).FindChild( "Text" ).GetComponent<TextMeshProUGUI>();
    }

    public void Start()
    {
        BeginDialogue( ToUse );
    }

    public void BeginDialogue( DialogueScene scene )
    {
        Session = new LinearStateMachine<ActorEntry>( ToUse.Entries );
        UpdateGraphics( Session.Current );
    }

    public void UpdateGraphics( ActorEntry entry )
    {
        SpeakerText.text = entry.Speaker;
        DialogueText.text = entry.Dialogue;
    }

    public void OnSubmit( BaseEventData eventData )
    {
        ActorEntry entry = Session.Next();
        if ( entry == null )
        {
            this.enabled = false;
            EventSystem.current.SetSelectedGameObject( BattleSystem.Instance.gameObject );
            this.GetComponent<Canvas>().enabled = false;
        }
        else
            UpdateGraphics( entry );
    }
}

[CreateAssetMenu]
public class DialogueScene : ScriptableObject
{
    public List<ActorEntry> Entries;
}

[Serializable]
public class ActorEntry 
{
    public string Speaker;
    public string Dialogue;
    public Sprite ActorSprite;
    public EntryActions Actions;
}

public enum EntryActions
{
    Enter,
    Exit,
}

public class LinearStateMachine<T> where T : class
{
    private LinkedList<T> Entries;
    private LinkedListNode<T> current;
    public T Current { get { return current.Value; } }

    public LinearStateMachine( IEnumerable<T> entries )
    {
        Entries = new LinkedList<T>( entries as IEnumerable<T> );
        current = Entries.First;
    }

    public T Next()
    {
        if ( current.Next == null )
            return null;
        else
        {
            current = current.Next;
            return current.Value;
        }
    }

    public T Previous()
    {
        if ( current.Previous == null )
            return null;
        else
        {
            current = current.Previous;
            return current.Value;
        }
    }
}