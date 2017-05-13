using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IStartTurnHandler
{
    void OnTurnStart();
}

[RequireComponent( typeof( WaitAbility ) )]
public class Unit : MonoBehaviour, IUnitDamagedHandler
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
                if ( UnitChanged != null )
                    UnitChanged( this );
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
                if ( UnitChanged != null )
                    UnitChanged( this );
            }
        }
    }

    public int MovementRange;

    public event Action<Unit> UnitChanged;

    [HideInInspector]
    public List<Ability> Abilities = new List<Ability>();

    public void Awake()
    {
        Abilities = GetComponents<Ability>().ToList();
    }

    public void Start()
    {
        UnitChanged( this );
    }

    public void UnitDamaged( int preDamage )
    {
        if ( HP <= 0 ) Die();
    }

    public void Die()
    {
        ExecuteEvents.ExecuteHierarchy<IUnitDeathHandler>
           ( this.gameObject, null, ( x, y ) => x.OnUnitDeath( this ) );
    }

    public void RegisterTurnController( TurnController controller )
    {
        GetComponentInChildren<SpriteRenderer>().color = controller.PlayerColor;
    }
}