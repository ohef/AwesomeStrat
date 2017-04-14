using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public abstract class TurnController : MonoBehaviour
{
    public HashSet<Unit> ControlledUnits;
    public HashSet<Unit> HasNotActed;
    public int PlayerNo;
    public Material PlayerMaterial;

    public void Awake()
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
}
