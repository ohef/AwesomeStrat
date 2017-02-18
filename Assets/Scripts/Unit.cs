using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Map
{
    public class IntChangedEvent : UnityEvent<int> { }

    [System.Serializable]
    public class Unit : MonoBehaviour
    {
        [SerializeField]
        private int m_HP;
        public int HP {
            get { return m_HP; }
            set
            {
                if ( m_HP != value )
                {
                    m_HP = value;
                    HPChanged.Invoke( m_HP );
                    UnitChanged.Invoke();
                }
            }
        }

        [SerializeField]
        private int m_MaxHP;
        public int MaxHP
        {
            get { return m_MaxHP; }
            set
            {
                if ( m_MaxHP != value )
                {
                    m_MaxHP = value;
                    MaxHPChanged.Invoke( m_MaxHP );
                    UnitChanged.Invoke( );
                }
            }
        }

        public int Defense;
        public int Attack;
        public int MovementRange;
        public int AttackRange;

        public IntChangedEvent HPChanged;
        public IntChangedEvent MaxHPChanged;
        public UnityEvent UnitChanged;

        void OnDrawGizmos()
        {
            Gizmos.DrawCube( this.transform.position, Vector3.one * 0.5f );
        }

        void Awake()
        {
            HPChanged = new IntChangedEvent();
            MaxHPChanged = new IntChangedEvent();

            if ( UnitChanged == null )
                UnitChanged = new UnityEvent();
        }

        void Start()
        {
            UnitChanged.Invoke();
        }
    }
}
