using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.General.DataStructures;
using UnityEngine;

namespace Assets.Map
{
    //public class Unit : MonoBehaviour
    public class Unit
    {
        public int HP { get; set; }
        public int Defense { get; set; }
        public int Attack { get; set; }
        public int Movement { get; set; }
        public Vector2Int Position { get; set; }
        public int AttackRange { get; set; }
    }
}
