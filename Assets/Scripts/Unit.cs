using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IStartTurnHandler
{
    void OnTurnStart();
}

[RequireComponent( typeof( WaitAbility ) )]
public class Unit : MonoBehaviour, IUnitDamagedHandler, IStartTurnHandler
{
    private int m_HP;
    public int HP
    {
        get { return m_HP; }
        set
        {
            if ( m_HP != value )
            {
                m_HP = value;
                UnitChanged( this );
            }
        }
    }

    private int m_MaxHP;
    public int MaxHP
    {
        get { return m_MaxHP; }
        set
        {
            if ( m_MaxHP != value )
            {
                m_MaxHP = value;
                UnitChanged( this );
            }
        }
    }

    public int Green
    {
        get { return green; }

        set { green = value; }
    }

    public int Black
    {
        get { return black; }

        set { black = value; }
    }

    public int Red
    {
        get { return red; }

        set { red = value; }
    }

    public int Blue
    {
        get { return blue; }

        set { blue = value; }
    }

    public int White
    {
        get { return white; }

        set { white = value; }
    }

    [SerializeField]
    private int green;
    [SerializeField]
    private int black;
    [SerializeField]
    private int red;
    [SerializeField]
    private int blue;
    [SerializeField]
    private int white;

    public int MovementRange;

    public event Action<Unit> UnitChanged;

    [HideInInspector]
    public List<Ability> Abilities = new List<Ability>();

    public int AttackPower { get { return Black / 2 + Red + White / 2 + Blue / 3 + Green; } }

    public void Awake()
    {
        Abilities = GetComponents<Ability>().ToList();
        HP = MaxHP = Black * 2 + Red + White + Blue / 2 + Green * 3;
    }

    public void Start()
    {
        UnitChanged( this );
    }

    public void UnitDamaged( int preDamage )
    {
        if ( HP <= 0 ) Die();
        else
        {
            PendingEffects.Add(
                new EffectEntry
                {
                    CountDown = 2,
                    OnFinish = () => HP = HP + preDamage >= MaxHP ? MaxHP : preDamage
                } );
        }
    }

    private List<EffectEntry> PendingEffects = new List<EffectEntry>();
    class EffectEntry
    {
        public int CountDown;
        public Action OnFinish;
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

    public void OnTurnStart()
    {
        foreach ( var effect in PendingEffects )
        {
            effect.CountDown -= 1;
            if ( effect.CountDown == 0 )
                effect.OnFinish();
        }
        PendingEffects.RemoveAll( effect => effect.CountDown == 0 );
    }
}