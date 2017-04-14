using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class IntChangedEvent : UnityEvent<int> { }

[RequireComponent( typeof( WaitAbility ) )]
public class Unit : MonoBehaviour, IUnitEventsHandler
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
    public int MovementRange;

    [HideInInspector]
    public List<Ability> Abilities = new List<Ability>(); 

    public UnityEvent UnitChanged;

    public Animator animationController;

    void Awake()
    {
        if ( UnitChanged == null )
            UnitChanged = new UnityEvent();

        Abilities = GetComponents<Ability>().ToList();
        animationController = this.GetComponentInChildren<Animator>();
    }

    void Start()
    {
        UnitChanged.Invoke();
    }

    public void UnitDamaged( UnitEventData data, int preDamage )
    {
        animationController.SetTrigger( "Damaged" );
        if ( HP <= 0 ) Die();
    }

    public void Die()
    {
        var hey = ExecuteEvents.ExecuteHierarchy<IUnitDeathHandler>
            ( this.gameObject, null, ( x, y ) => x.OnUnitDeath( this ) );
        Debug.Log( hey );
    }
}

interface IUnitDeathHandler : IEventSystemHandler
{
    void OnUnitDeath( Unit deadUnit );
}