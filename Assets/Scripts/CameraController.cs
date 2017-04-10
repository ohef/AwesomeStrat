using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.General.DataStructures;

namespace TacticsGame
{
    [RequireComponent( typeof( Camera) )]
    public class CameraController : MonoBehaviour
    {
        private Camera MapCamera; 

        void Awake()
        {
            MapCamera = GetComponent<Camera>();
        }

        private void Start()
        {
            foreach ( KeyValuePair<Unit, Vector2Int> kvp in BattleSystem.Instance.Map.UnitPos )
            {
                kvp.Key.transform.GetChild( 0 ).rotation = MapCamera.transform.rotation;
            }
        }

        public void RotateFortyFiveDegrees()
        {
             transform.RotateAround( transform.parent.position, Vector3.up, 45.0f );
        }
    }
}