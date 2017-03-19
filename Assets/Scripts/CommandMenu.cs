using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class CommandMenu : MonoBehaviour {
    private List<Button> m_Buttons = new List<Button>();

    public void AddButton( Button button )
    {
        button.transform.SetParent( transform );
        m_Buttons.Add( button );
    }
    
    public void AddButtons( IEnumerable<Button> buttons )
    {
        foreach ( var button in buttons )
            this.AddButton( button );
    }

    public Button AddButton( string text, UnityEngine.Events.UnityAction onClick )
    {
        Button toadd = Instantiate( AbilityButtonFactory.instance.DefaultButton );
        toadd.GetComponentInChildren<Text>().text = text;
        toadd.onClick.AddListener( onClick );
        toadd.name = text;
        toadd.transform.SetParent( transform );
        m_Buttons.Add( toadd );

        //LinkButtonNavigation();
        return toadd;
    }

    public void ShowButtons()
    {
        foreach ( var button in m_Buttons )
        {
            button.gameObject.SetActive( true );
        }
    }

    public void HideButtons()
    {
        foreach ( var button in m_Buttons )
        {
            button.gameObject.SetActive( false );
        }
    }

    private void LinkButtonNavigation()
    {
        IEnumerator<Button> enumerator = m_Buttons.GetEnumerator();
        enumerator.MoveNext();
        while ( true )
        {
            Button first = enumerator.Current;
            if ( enumerator.MoveNext() )
            {
                Button second = enumerator.Current;
                NavigateToEachother( first, second );
            }
            else
            {
                Button second = m_Buttons[ 0 ];
                NavigateToEachother( first, second );
                break;
            }
        }
    }

    private static void NavigateToEachother( Button first, Button second )
    {
        Navigation firstNav = first.navigation;
        first.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = second, selectOnUp = firstNav.selectOnUp };

        Navigation secondNav = second.navigation;
        second.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = secondNav.selectOnDown, selectOnUp = first };
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
