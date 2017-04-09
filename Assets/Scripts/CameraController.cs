using UnityEngine;
using System.Collections;

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
            foreach ( Unit unit in BattleSystem.Instance.Map.UnitPos )
            {
                unit.transform.GetChild( 0 ).rotation = MapCamera.transform.rotation;
            } 
        }

        public void RotateFortyFiveDegrees()
        {
             transform.RotateAround( transform.parent.position, Vector3.up, 45.0f );
        }
    }
}