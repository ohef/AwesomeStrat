using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.General.DataStructures;

namespace Assets.Map
{
    [Serializable]
    public class Unit
    {
        public int HP;
        public int Defense;
        public int Attack;
        public int Movement;
        public Vector2Int Position;
        public int AttackRange;
    }
}
