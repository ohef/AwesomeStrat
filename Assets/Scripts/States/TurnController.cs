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
    public Material PlayerMaterial;

    public virtual void Awake()
    {
        ControlledUnits = new HashSet<Unit>( this.transform.GetComponentsInChildren<Unit>() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );

        //TODO probably don't need to do this, better to do one assignment.
        foreach ( var unit in ControlledUnits )
        {
            unit.GetComponent<UnitGraphics>()
                .UnitIndicator.sharedMaterial = PlayerMaterial;
        }
    }

    public void OnUnitDeath( Unit deadUnit )
    {
        ControlledUnits.Remove( deadUnit );
        HasNotActed.Remove( deadUnit );
        BattleSystem.Instance.Map.UnitPos.Remove( deadUnit );
        GameObject.Destroy( deadUnit.gameObject );
    }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
}
