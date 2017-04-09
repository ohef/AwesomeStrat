﻿using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ChooseTargetsState : BattleState
{
    private LinkedList<Unit> EligibleTargets;
    private LinkedListNode<Unit> CurrentlySelected;
    private TargetAbility SelectedAbility;

    public static BattleState Create( TargetAbility ability )
    {
        return new CancelableState( new ChooseTargetsState( ability ), false );
    }

    public ChooseTargetsState( TargetAbility ability )
    {
        SelectedAbility = ability;
    }

    public override void Enter( PlayerTurnController context )
    {
        var unitsInRange = sys.Map.GetUnitsWithinRange( 
            sys.Map.UnitPos[ SelectedAbility.Owner ],
            SelectedAbility.Range );

        EligibleTargets = new LinkedList<Unit>(
            SelectedAbility.GetInteractableUnits( unitsInRange,
                SelectedAbility.GetTargetPredicate( context ) ) );
        CurrentlySelected = EligibleTargets.First;
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
    }

    public override void Update( PlayerTurnController context )
    {
        HandleChoosing();

        if ( Input.GetButtonDown( "Submit" ) )
        {
            SelectedAbility.ExecuteOnTarget( CurrentlySelected.Value );
            context.GoToStateAndForget( ChoosingUnitState.Instance );
            context.UnitFinished( SelectedAbility.Owner );
        }
    }

    public override void Exit( PlayerTurnController context )
    {
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ SelectedAbility.Owner ] );
    }

    private void HandleChoosing()
    {
        Vector2Int input = Vector2IntExt.GetInputAsDiscrete();
        if ( EligibleTargets.Count > 0 )
        {
            HandleInput( input.x );
            HandleInput( input.y );
        }
    }

    private void HandleInput( int input )
    {
        if ( input == 1 )
        {
            if ( CurrentlySelected.Next == null )
                CurrentlySelected = EligibleTargets.First;
            else
                CurrentlySelected = CurrentlySelected.Next;

            sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
        }
        else if ( input == -1 )
        {
            if ( CurrentlySelected.Previous == null )
                CurrentlySelected = EligibleTargets.Last;
            else
                CurrentlySelected = CurrentlySelected.Previous;

            sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
        }
    }
}