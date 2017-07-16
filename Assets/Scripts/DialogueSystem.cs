using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.General;

public class DialogueSystem : MonoBehaviour, ISubmitHandler
{
    public DialogueScene ToUse;
    public ReversableIterator<ActorEntry> Session;
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
        Session = new ReversableIterator<ActorEntry>( ToUse.Entries );
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