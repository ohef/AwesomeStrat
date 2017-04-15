using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.General.DataStructures;

[RequireComponent( typeof( Camera ) )]
public class CameraController : MonoBehaviour
{
    private Camera MapCamera;

    void Awake()
    {
        MapCamera = GetComponent<Camera>();
    }

    private void Start() { }

    public void RotateFortyFiveDegrees()
    {
        transform.RotateAround( transform.parent.position, Vector3.up, 45.0f );
    }
}