using UnityEngine;
using System.Collections;

public class FocusOnCamera : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        if ( Camera.current != null )
        {
            transform.rotation = Camera.current.transform.rotation;
        }
	}
}
