using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

sealed class ChooseTargetsState : BattleState
    , IMoveHandler, ISubmitHandler
    , IPointerDownHandler
{
    LinkedList<Unit> EligibleTargets;
    LinkedListNode<Unit> CurrentlySelected;
    TargetAbility SelectedAbility;
    Unit SelectedUnit;

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
        Unit unitAtCursor;
        if (
        sys.Map.UnitPos.TryGetValue( sys.Cursor.CurrentPosition, out unitAtCursor ) &&
        EligibleTargets.Contains( unitAtCursor ) )
        {
            SelectedAbility.ExecuteOnTarget( SelectedUnit, unitAtCursor );
            sys.GoToState( sys.GetState<ChoosingUnitState>() );
            sys.UnitFinished( SelectedUnit );
        }
    }

    public override void Enter()
    {
        var unitsInRange = sys.Map.GetUnitsWithinRange(
            sys.Map.UnitPos[ SelectedUnit ],
            SelectedAbility.Range );

        EligibleTargets = new LinkedList<Unit>(
            unitsInRange.Where( SelectedAbility.CanTargetFunction( SelectedUnit, sys.CurrentTurnController ) ) );

        foreach ( var target in EligibleTargets )
            target.GetComponentInChildren<SpriteRenderer>().material.EnableKeyword( "SHADECOLOR_ON" );

        CurrentlySelected = EligibleTargets.First;
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
    }

    public override void Exit()
    {
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ SelectedUnit ] );

        foreach ( var target in EligibleTargets )
            target.GetComponentInChildren<SpriteRenderer>().material.DisableKeyword( "SHADECOLOR_ON" );
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        var unit = eventData.pointerPressRaycast.gameObject.GetComponent<Unit>();
        if ( unit != null && EligibleTargets.Contains( unit ) )
        {
            SelectedAbility.ExecuteOnTarget( SelectedUnit, unit );

            sys.GoToState( sys.GetState<ChoosingUnitState>() );
            sys.UnitFinished( SelectedUnit );
        }
        else
        {
            ExecuteEvents.Execute( gameObject, eventData, ExecuteEvents.cancelHandler );
        }
    }
}