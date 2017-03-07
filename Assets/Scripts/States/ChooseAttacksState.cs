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

    public override void Update( BattleSystem sys )
    {
        HandleChoosing( sys );

        if ( Input.GetButtonDown( "Submit" ) )
        {
            AttackUnit( SelectedAttackingUnit, CurrentlySelected.Value );
            sys.CurrentTurn.GoToStateAndForget( ChoosingUnitState.Instance );
            sys.CurrentTurn.UnitFinished( SelectedAttackingUnit );
        }
    }

    public void AttackUnit( Unit attackingUnit, Unit otherUnit )
    {
        otherUnit.GetComponentInChildren<Animator>().SetTrigger( "Damaged" );
        attackingUnit.GetComponentInChildren<Animator>().SetTrigger( "Attack" );
        otherUnit.HP -= attackingUnit.Attack - otherUnit.Defense;
    }

    public override void Exit( BattleSystem sys )
    {
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedAttackingUnit ].Position );
    }

    public override void Enter( BattleSystem sys )
    {
        sys.Map.ShowStandingAttackRange( SelectedAttackingUnit );
    }

    private void HandleChoosing( BattleSystem sys )
    {
        Vector2Int input = Vector2IntExt.GetInputAsDiscrete();
        if ( ToAttack.Count > 0 )
        {
            HandleInput( sys, input );
        }
    }

    private void HandleInput( BattleSystem sys, Vector2Int input )
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

