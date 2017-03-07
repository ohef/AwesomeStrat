using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseAttacksState : BattleState
{
    private MapUnit UnitMakingAttacks;
    private LinkedList<MapUnit> ToAttack;
    private LinkedListNode<MapUnit> CurrentlySelected;

    public ChooseAttacksState( MapUnit selectedUnit, IEnumerable<MapUnit> toAttack )
    {
        UnitMakingAttacks = selectedUnit;
        ToAttack = new LinkedList<MapUnit>( toAttack );
        CurrentlySelected = ToAttack.First;
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
    }

    public override void Update( BattleSystem sys )
    {
        if ( Input.GetButtonDown( "Cancel" ) )
        {
            sys.CurrentTurn.GoToPreviousState();
        }

        HandleChoosing( sys );

        if ( Input.GetButtonDown( "Submit" ) )
        {
            AttackUnit( UnitMakingAttacks, CurrentlySelected.Value );
            sys.CurrentTurn.GoToStateAndForget( ChoosingUnitState.Instance );
            UnitMakingAttacks.hasTakenAction = true;
        }
    }

    public void AttackUnit( MapUnit attackingUnit, MapUnit otherUnit )
    {
        otherUnit.GetComponentInChildren<Animator>().SetTrigger( "Damaged" );
        attackingUnit.GetComponentInChildren<Animator>().SetTrigger( "Attack" );
        otherUnit.HP -= attackingUnit.Attack - otherUnit.Defense;
    }

    public override void Exit( BattleSystem sys )
    {
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ UnitMakingAttacks ].Position );
    }

    public override void Enter( BattleSystem sys )
    {
        sys.Map.ShowStandingAttackRange( UnitMakingAttacks );
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

