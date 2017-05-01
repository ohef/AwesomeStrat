using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class IntChangedEvent : UnityEvent<int> { }

[RequireComponent( typeof( WaitAbility ) )]
public class Unit : MonoBehaviour, IUnitDamagedHandler
//public class Unit : MonoBehaviour, IUnitDamagedHandler, IEffectProvider
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

    public int AttackPower { get { return Black + Red + White + Blue + Green; } }

    public int MovementRange;

    [HideInInspector]
    public List<Ability> Abilities = new List<Ability>();

    public event Action<Unit> UnitChanged;

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

//public interface IEffectProvider
//{
//    int Green { get; set; }
//    int Blue { get; set; }
//    int Black { get; set; }
//    int White { get; set; }
//    int Red { get; set; }
//}

//public interface ICanApplyEffect
//{
//    void ApplyEffect( IEffectProvider effects );
//}

//public struct EffectBlock : IEffectProvider
//{
//    public int Green { get; set; }
//    public int Blue  { get; set; }
//    public int Black { get; set; }
//    public int White { get; set; }
//    public int Red   { get; set; }

//    public static EffectBlock AddEffect( IEffectProvider b, IEffectProvider a )
//    {
//        return new EffectBlock
//        {
//            Green = a.Green + b.Green,
//            Red   = a.Red   + b.Red,
//            Blue  = a.Blue  + b.Blue,
//            White = a.White + b.White,
//            Black = a.Black + b.Black,
//        };
//    }
//}