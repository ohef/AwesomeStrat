using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSystem : MonoBehaviour {
    private CommandMenu menu;

    public void Awake()
    {
        menu = this.GetComponentInChildren<CommandMenu>();
    }

    public void LoadScene( string scene )
    {
        SceneManager.LoadScene( scene );
    }
}
