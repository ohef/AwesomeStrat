using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CursorEffects : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}

    [SerializeField]
    private GameObject CursorEffect;
	
	// Update is called once per frame
	void Update () {
        if ( Input.GetMouseButtonDown( 0 ) )
        {
            var mousePosition = Input.mousePosition;
            mousePosition += Vector3.forward * 5;
            var obj = GameObject.Instantiate( CursorEffect, GetComponent<Camera>().ScreenToWorldPoint( mousePosition ), Quaternion.identity );
            StartCoroutine( WaitThenDo( 1.0f, () => GameObject.Destroy( obj ) ) );
        }
    }

    IEnumerator WaitThenDo( float seconds, Action actionToTake )
    {
        yield return new WaitForSeconds( seconds );
        actionToTake();
    }
}
