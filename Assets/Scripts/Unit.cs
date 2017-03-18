using System;
using System.Collections.Generic;
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

    public Ability[] Abilities;

    public UnityEvent UnitChanged;

    void Awake()
    {
        if ( UnitChanged == null )
            UnitChanged = new UnityEvent();

        Abilities = new Ability[] {
            new WaitAbility { Name = "Wait", Owner = this },
            new AttackAbility { Name = "Attack", Owner = this, Range = AttackRange }
        };
    }

    void Start()
    {
        UnitChanged.Invoke();
    }
}
