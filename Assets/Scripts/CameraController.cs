using UnityEngine;
using System.Collections;

namespace TacticsGame
{
    [RequireComponent( typeof( Camera) )]
    public class CameraController : MonoBehaviour
    {
        private Camera camera; 

        void Awake()
        {
            camera = GetComponent<Camera>();
        }

        public void RotateFortyFiveDegrees()
        {
             transform.RotateAround( transform.parent.position, Vector3.up, 45.0f );
        }
    }
}