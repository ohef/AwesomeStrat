using Assets.General;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Scripts.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MapCameraController : MonoBehaviour {

    public float CameraSpeed;

    private Camera MapCam;
    private IEnumerator<float> ZoomLevels;
    private CursorControl Cursor;
    private Vector3 CurrentVelocity = Vector3.zero;
    private Vector3 Target;

    public void Awake()
    {
        MapCam = GetComponent<Camera>();
    }

    public void Start()
    {
        Cursor = BattleSystem.Instance.Cursor;
        Target = KeepMapInFocus( Cursor.transform.TransformPoint( Cursor.CurrentPosition.ToVector3( axisVal: -10 ) ) );

        //TODO: Maybe it's unnecessary to cache this 
        MouseAtScreenBoundaries = CreateScreenBoundaryCheck();

        ZoomLevels = GetNextZoom().GetEnumerator();
        ZoomLevels.MoveNext();
    }

    private IEnumerable<float> GetNextZoom()
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

        //TODO Don't compute this at runtime?
        float orthographicSizeWidth = Camera.main.orthographicSize * Screen.width / Screen.height;

        toClamp.x = CustomMath.ClampNumber( toClamp.x, orthographicSizeWidth - 0.5f, map.Width - orthographicSizeWidth - 0.5f );
        toClamp.y = CustomMath.ClampNumber( toClamp.y, MapCam.orthographicSize - 0.5f, map.Height - MapCam.orthographicSize - 0.5f );
        return toClamp;
    }

    private Func<bool> MouseAtScreenBoundaries;

    //TODO: Don't keep computing this every call
    //private bool MouseAtScreenBoundaries( int margin = 10 )
    //{
    //    var origin = new Vector2( margin, margin );
    //    var point = Input.mousePosition.ToVector2() - origin;
    //    var rectHeight = new Vector2( margin, Screen.height - margin ) - origin;
    //    var rectWidth = new Vector2( Screen.width - margin, margin ) - origin;
    //    Rect ScreenRect = new Rect( origin, new Vector2( rectWidth.magnitude, rectHeight.magnitude ) );

    //    return ScreenRect.Contains( point ) == false;
    //}

    public void Update()
    {
        if ( MouseAtScreenBoundaries() )
            MoveInDirectionOfMouse();

        transform.position = Vector3.SmoothDamp(
            transform.position, Target, ref CurrentVelocity, CameraSpeed );
    }

    public void DoNextZoom()
    {
        float a = ZoomLevels.Current;
        ZoomLevels.MoveNext();
        float b = ZoomLevels.Current;
        StartCoroutine( CustomAnimation.InterpolateValue( a, b, 0.22f, val => MapCam.orthographicSize = val ) );
    }

    private Func<bool> CreateScreenBoundaryCheck( int margin = 10 )
    {
        var origin = new Vector2( margin, margin );
        var rectHeight = new Vector2( margin, Screen.height - margin ) - origin;
        var rectWidth = new Vector2( Screen.width - margin, margin ) - origin;
        Rect ScreenRect = new Rect( origin, new Vector2( rectWidth.magnitude, rectHeight.magnitude ) );

        return delegate
        {
            var point = Input.mousePosition.ToVector2() - origin;
            return ScreenRect.Contains( point ) == false;
        };
    }

    private void MoveInDirectionOfMouse()
    {
        //Position relative to the center of the screen 
        int deltaFactor = 4;
        Vector2 centerPos = Input.mousePosition.ToVector2() - new Vector2( Screen.width / 2, Screen.height / 2 );
        Vector3 translated = KeepMapInFocus( MapCam.transform.position.ToVector2() + centerPos.normalized * deltaFactor );
        translated.Set( translated.x, translated.y, MapCam.transform.position.z );
        Target = translated;
    }

    public void OnCursorMoved()
    {
        if ( Cursor == null ) return;

        Target = KeepMapInFocus( Cursor.CurrentPosition.ToVector3( axisVal: -10 ) );
    }
}
