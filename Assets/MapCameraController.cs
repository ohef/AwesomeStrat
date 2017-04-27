using Assets.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraController : MonoBehaviour {

    private Camera MapCam;
    private IEnumerator<float> ZoomLevels;


    public void Awake()
    {
        MapCam = GetComponent<Camera>();
        ZoomLevels = GetNextZoom().GetEnumerator();
        ZoomLevels.MoveNext();
    }

    public IEnumerable<float> GetNextZoom()
    {
        while ( true )
        {
            yield return 5.0f;
            yield return 10.0f;
            yield return 15.0f;
        }
    }

    public void Update()
    {
        if ( Input.GetKeyUp( KeyCode.Semicolon ) )
        {
            float a = ZoomLevels.Current;
            ZoomLevels.MoveNext();
            float b = ZoomLevels.Current;
            StartCoroutine( CustomAnimation.InterpolateValue( a, b, 0.22f, val => MapCam.orthographicSize = val ) );
        }
    }
}
