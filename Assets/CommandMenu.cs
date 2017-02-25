using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class CommandMenu : MonoBehaviour {
    public Button DefaultButton;
    private List<Button> m_Buttons = new List<Button>();

    public Button AddButton( string text, UnityEngine.Events.UnityAction onClick )
    {
        Button toadd = Instantiate( DefaultButton );
        toadd.GetComponentInChildren<Text>().text = text;
        toadd.onClick.AddListener( onClick );
        toadd.transform.SetParent( transform );
        m_Buttons.Add( toadd );
        return toadd;
    }

    public void ClearButtons()
    {
        foreach ( var button in m_Buttons )
        {
            GameObject.Destroy( button.gameObject );
        }
        m_Buttons.Clear();
    }
}
