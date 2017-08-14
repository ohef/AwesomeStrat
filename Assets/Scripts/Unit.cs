using Assets.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IUnitMemento, IUnitDamagedHandler
    //, IBeginDragHandler, IDragHandler, IEndDragHandler
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
    public IEnumerable<IAbility> Abilities { get { return Class.GetActiveAbilities( this ); } }

    public void Start()
    {
        if ( UnitChanged != null )
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

    #region Memento Implementation
    public void SetMemento( UnitMemento memento )
    {
        SetPositionFromMemento( memento );
        SetMaterialBlockFromMemento( memento );
    }

    public UnitMemento CreateMemento()
    {
        return new UnitMemento( this );
    }

    public void SetPositionFromMemento( UnitMemento memento )
    {
        transform.position = memento.Position;
    }

    public void SetMaterialBlockFromMemento( UnitMemento memento )
    {
        GetComponent<Renderer>().SetPropertyBlock( memento.MaterialProperty );
    }
    #endregion

    //UnitMemento BeforeDrag;
    //public void OnBeginDrag( PointerEventData eventData )
    //{
    //    BeforeDrag = CreateMemento();
    //}

    //public void OnDrag( PointerEventData eventData )
    //{
    //    transform.position = eventData.pointerCurrentRaycast.worldPosition;
    //}

    //public void OnEndDrag( PointerEventData eventData )
    //{
    //    StartCoroutine( CustomAnimation.InterpolateToPoint( transform, BeforeDrag.Position, 0.25f ) );
    //}
}