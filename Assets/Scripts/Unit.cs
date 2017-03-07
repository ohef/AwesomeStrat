using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class IntChangedEvent : UnityEvent<int> { }

[Serializable]
public class Unit : MonoBehaviour
{
    [SerializeField]
    private int m_HP;
    public int HP
    {
        get { return m_HP; }
        set
        {
            if ( m_HP != value )
            {
                m_HP = value;
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
                UnitChanged.Invoke();
            }
        }
    }

    public int Defense;
    public int Attack;
    public int MovementRange;
    public int AttackRange;

    public UnityEvent UnitChanged;

    void Awake()
    {
        if ( UnitChanged == null )
            UnitChanged = new UnityEvent();
    }

    void Start()
    {
        UnitChanged.Invoke();
    }
}
