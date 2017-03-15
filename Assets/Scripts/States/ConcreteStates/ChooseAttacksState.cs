using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseAttacksState : BattleState
{
    private Unit SelectedAttackingUnit;
    private LinkedList<Unit> ToAttack;
    private LinkedListNode<Unit> CurrentlySelected;

    public static BattleState Create( Unit selectedUnit, IEnumerable<Unit> toAttack )
    {
        return new CancelableState( new ChooseAttacksState( selectedUnit, toAttack ), false );
    }

    public ChooseAttacksState( Unit selectedUnit, IEnumerable<Unit> toAttack )
    {
        SelectedAttackingUnit = selectedUnit;
        ToAttack = new LinkedList<Unit>( toAttack );
        CurrentlySelected = ToAttack.First;
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
    }

    public override void Update( TurnState context )
    {
        HandleChoosing();

        if ( Input.GetButtonDown( "Submit" ) )
        {
            AttackUnit( SelectedAttackingUnit, CurrentlySelected.Value );
            context.GoToStateAndForget( ChoosingUnitState.Instance );
            context.UnitFinished( SelectedAttackingUnit );
        }
    }

    public override void Exit( TurnState context )
    {
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedAttackingUnit ].Position );
    }

    public override void Enter( TurnState context )
    {
        sys.Map.ShowStandingAttackRange( SelectedAttackingUnit );
    }

    public void AttackUnit( Unit attackingUnit, Unit otherUnit )
    {
        otherUnit.GetComponentInChildren<Animator>().SetTrigger( "Damaged" );
        attackingUnit.GetComponentInChildren<Animator>().SetTrigger( "Attack" );
        otherUnit.HP -= attackingUnit.Attack - otherUnit.Defense;
    }

    private void HandleChoosing()
    {
        Vector2Int input = Vector2IntExt.GetInputAsDiscrete();
        if ( ToAttack.Count > 0 )
        {
            HandleInput( input );
        }
    }

    private void HandleInput( Vector2Int input )
    {
        if ( input.x == 1 )
        {
            if ( CurrentlySelected.Next == null )
                CurrentlySelected = ToAttack.First;
            else
                CurrentlySelected = CurrentlySelected.Next;

            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
        }
        else if ( input.x == -1 )
        {
            if ( CurrentlySelected.Previous == null )
                CurrentlySelected = ToAttack.Last;
            else
                CurrentlySelected = CurrentlySelected.Previous;

            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
        }
    }
}

