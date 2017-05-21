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

    public string Name;
    public int MovementRange;

    public int White; 
    public int Blue; 
    public int Black; 
    public int Red; 
    public int Green; 

    public event Action<Unit> UnitChanged;

    public UnitClass Class;
    public IEnumerable<Ability> Abilities { get { return Class.GetActiveAbilities( this ); } }

    public void Awake()
    {
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

[CreateAssetMenu]
public class UnitClass : ScriptableObject
{
    public string Name;
    public List<Ability> ClassAbilities;

    public IEnumerable<Ability> GetActiveAbilities( Unit unit )
    {
        yield return WaitAbility.Instance;
        foreach ( var ability in ClassAbilities.Where( ability => ability.Useable( unit ) ) )
        {
            yield return ability;
        }
    }
}