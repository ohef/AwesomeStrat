using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

sealed class ChooseTargetsState : BattleState, IMoveHandler, ISubmitHandler
{
    private LinkedList<Unit> EligibleTargets;
    private LinkedListNode<Unit> CurrentlySelected;
    private TargetAbility SelectedAbility;
    private Unit SelectedUnit;

    public void Initialize( TargetAbility ability, Unit selectedUnit )
    {
        SelectedAbility = ability;
        SelectedUnit = selectedUnit;
    }

    private void HandleInput( int input )
    {
        if ( input == 1 ) 
        {
            if ( CurrentlySelected.Next == null )
                CurrentlySelected = EligibleTargets.First; 
            else
                CurrentlySelected = CurrentlySelected.Next;
        }
        else if ( input == -1 ) 
        {
            if ( CurrentlySelected.Previous == null )
                CurrentlySelected = EligibleTargets.Last; 
            else
                CurrentlySelected = CurrentlySelected.Previous;

        }
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
    }

    public void OnMove( AxisEventData eventData )
    {
        Vector2Int v = eventData.moveVector.ToVector2Int();
        HandleInput( v.x );
        HandleInput( v.y );
    }

    public void OnSubmit( BaseEventData eventData )
    {
        SelectedAbility.ExecuteOnTarget( SelectedUnit, CurrentlySelected.Value );
        Context.GoToStateAndForget( sys.GetState<ChoosingUnitState>() );
        Context.UnitFinished( SelectedUnit );
    }

    public override void Enter()
    {
        var unitsInRange = sys.Map.GetUnitsWithinRange(
            sys.Map.UnitPos[ SelectedUnit ],
            SelectedAbility.Range );

        EligibleTargets = new LinkedList<Unit>(
            unitsInRange.Where( SelectedAbility.CanTargetFunction( SelectedUnit, Context ) ) );

        CurrentlySelected = EligibleTargets.First;
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
        base.Enter();
    }

    public override void Exit()
    {
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ SelectedUnit ] );
        base.Exit();
    }
}