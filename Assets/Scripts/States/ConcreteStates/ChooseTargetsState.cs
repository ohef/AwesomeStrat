using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ChooseTargetsState : BattleState
{
    private LinkedList<Unit> EligibleTargets;
    private LinkedListNode<Unit> CurrentlySelected;
    private TargetAbility SelectedAbility;

    public void Initialize( TargetAbility ability )
    {
        SelectedAbility = ability;
    }

    public void OnEnable()
    {
        var unitsInRange = sys.Map.GetUnitsWithinRange( 
            sys.Map.UnitPos[ SelectedAbility.Owner ],
            SelectedAbility.Range );

        EligibleTargets = new LinkedList<Unit>(
            unitsInRange.Where( SelectedAbility.CanTargetFunction( Context ) ) );

        CurrentlySelected = EligibleTargets.First;
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
    }

    public void Update()
    {
        HandleChoosing();

        if ( Input.GetButtonDown( "Submit" ) )
        {
            SelectedAbility.ExecuteOnTarget( CurrentlySelected.Value );
            //context.GoToStateAndForget( ChoosingUnitState.Instance );
            Context.GoToStateAndForget( sys.GetState<ChoosingUnitState>() );
            Context.UnitFinished( SelectedAbility.Owner );
        }
    }

    public void OnDisable()
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