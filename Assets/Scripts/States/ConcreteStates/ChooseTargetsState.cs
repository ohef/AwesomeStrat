using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChooseTargetsState : BattleState, IMoveHandler, ISubmitHandler
{
    private LinkedList<Unit> EligibleTargets;
    private LinkedListNode<Unit> CurrentlySelected;
    private TargetAbility SelectedAbility;
    private Unit AbilityUser;

    public void Initialize( TargetAbility ability, Unit abilityUser )
    {
        SelectedAbility = ability;
        AbilityUser = abilityUser;
    }

    public void OnEnable()
    {
        var unitsInRange = sys.Map.GetUnitsWithinRange(
            sys.Map.UnitPos[AbilityUser],
            SelectedAbility.Range );

        EligibleTargets = new LinkedList<Unit>(
            unitsInRange.Where( SelectedAbility.CanTargetFunction( AbilityUser, Context ) ) );

        CurrentlySelected = EligibleTargets.First;
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ CurrentlySelected.Value ] );
        EventSystem.current.SetSelectedGameObject( gameObject );
    }

    public void OnDisable()
    {
        sys.Cursor.MoveCursor( sys.Map.UnitPos[ AbilityUser ] );
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

    public void OnMove( AxisEventData eventData )
    {
        if ( EligibleTargets.Count > 0 )
        {
            HandleInput( eventData.moveVector.ToVector2Int().x );
            HandleInput( eventData.moveVector.ToVector2Int().y );
            eventData.Use();
        }
    }

    public void OnSubmit( BaseEventData eventData )
    {
        SelectedAbility.ExecuteOnTarget( AbilityUser, CurrentlySelected.Value );
        Context.GoToStateAndForget( sys.GetState<ChoosingUnitState>() );
        Context.UnitFinished( AbilityUser );
        eventData.Use();
    }
}