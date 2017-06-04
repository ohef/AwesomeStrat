using Assets.General;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Scripts.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapCameraController : MonoBehaviour {

    public float CameraSpeed;

    private Camera MapCam;
    private IEnumerator<float> ZoomLevels;
    private CursorControl Cursor;
    private Vector3 CurrentVelocity = Vector3.zero;

    public void Awake()
    {
        MapCam = GetComponent<Camera>();
        ZoomLevels = GetNextZoom().GetEnumerator();
        ZoomLevels.MoveNext();
    }

    public void Start()
    {
        Cursor = BattleSystem.Instance.Cursor;
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

    private Vector3 KeepMapInFocus( Vector3 toClamp )
    {
        GameMap map = BattleSystem.Instance.Map;
        toClamp.x = CustomMath.ClampNumber( toClamp.x, MapCam.orthographicSize, map.Width - MapCam.orthographicSize - 1 );
        toClamp.y = CustomMath.ClampNumber( toClamp.y, MapCam.orthographicSize - 1, map.Height - MapCam.orthographicSize );
        return toClamp;
    }

    public void FixedUpdate()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            KeepMapInFocus( Cursor.transform.TransformPoint( new Vector3( 0, 0, -1 ) ) ),
            ref CurrentVelocity,
            CameraSpeed );

        if ( Input.GetKeyUp( KeyCode.Semicolon ) )
        {
            float a = ZoomLevels.Current;
            ZoomLevels.MoveNext();
            float b = ZoomLevels.Current;
            StartCoroutine( CustomAnimation.InterpolateValue( a, b, 0.22f, val => MapCam.orthographicSize = val ) );
        }
    }
}
