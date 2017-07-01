using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class TurnController : MonoBehaviour, IMonoBehaviourState, IUnitDeathHandler
{
    public HashSet<Unit> ControlledUnits;
    public HashSet<Unit> HasNotActed;

    public int PlayerNo;
    public Color PlayerColor;

    public virtual void Awake()
    {
        ControlledUnits = new HashSet<Unit>( this.transform.GetComponentsInChildren<Unit>() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );

        foreach ( var unit in ControlledUnits )
        {
            unit.RegisterTurnController( this );
        }
    }

    public void OnUnitDeath( Unit deadUnit )
    {
        ControlledUnits.Remove( deadUnit );
        HasNotActed.Remove( deadUnit );
        BattleSystem.Instance.Map.UnitPos.Remove( deadUnit );
        GameObject.Destroy( deadUnit.gameObject );
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void UpdateState() { }
}
