using Assets.General.DataStructures;
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

    public override void Enter( TurnState context )
    {
        Predicate<Unit> predicate = unit => true;
        if ( SelectedAbility.Targets == AbilityTargets.Enemy )
        {
            predicate = unit => !context.ControlledUnits.Contains( unit ) && unit != SelectedAbility.Owner;
        }
        else if ( SelectedAbility.Targets == AbilityTargets.Friendly )
        {
            predicate = unit => context.ControlledUnits.Contains( unit ) && unit != SelectedAbility.Owner;
        }

        EligibleTargets = new LinkedList<Unit>( GetInteractableUnits( SelectedAbility, predicate ) );
        CurrentlySelected = EligibleTargets.First;
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
    }

    public override void Update( TurnState context )
    {
        HandleChoosing();

        if ( Input.GetButtonDown( "Submit" ) )
        {
            SelectedAbility.ExecuteOnTarget( CurrentlySelected.Value );
            context.GoToStateAndForget( ChoosingUnitState.Instance );
            context.UnitFinished( SelectedAbility.Owner );
        }
    }

    public override void Exit( TurnState context )
    {
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedAbility.Owner ].Position );
    }

    private void HandleChoosing()
    {
        Vector2Int input = Vector2IntExt.GetInputAsDiscrete();
        if ( EligibleTargets.Count > 0 )
        {
            HandleInput( input );
        }
    }

    private void HandleInput( Vector2Int input )
    {
        if ( input.x == 1 )
        {
            if ( CurrentlySelected.Next == null )
                CurrentlySelected = EligibleTargets.First;
            else
                CurrentlySelected = CurrentlySelected.Next;

            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
        }
        else if ( input.x == -1 )
        {
            if ( CurrentlySelected.Previous == null )
                CurrentlySelected = EligibleTargets.Last;
            else
                CurrentlySelected = CurrentlySelected.Previous;

            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
        }
    }

    private IEnumerable<Unit> 
        GetInteractableUnits( TargetAbility ability, Predicate<Unit> UseableOn )
    {
        foreach ( var tile in sys.Map.GetTilesWithinAbsoluteRange(
            sys.Map.UnitGametileMap[ ability.Owner ].Position, ability.Range ) )
        {
            Unit unitToCheck = null;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Map[ tile ], out unitToCheck ) )
            {
                if ( UseableOn( unitToCheck ) )
                    yield return unitToCheck;
                else
                    continue;
            }
        }
        yield break;
    }
}