using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.General.DataStructures;
using UnityEngine;

namespace Assets.Map
{
    public class Unit : MonoBehaviour
    {
        public int HP;
        public int Defense;
        public int Attack;
        public int Movement;
        public int AttackRange;

        void OnDrawGizmos()
        {
            Gizmos.DrawCube( this.transform.position, Vector3.one * 0.5f );
        }
    }
}
