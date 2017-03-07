using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosingUnitActionsState : MenuState
{
    private Unit SelectedUnit;
    private GameTile SelectedTile { get { return sys.Map.UnitGametileMap[ SelectedUnit ]; } }
    private bool MoveTaken;

    public static BattleState Create( Unit selectedUnit, bool movetaken = false )
    {
        return new CancelableState( new ChoosingUnitActionsState( selectedUnit, movetaken ) );
    }

    private ChoosingUnitActionsState( Unit selectedUnit, bool moveTaken = false )
    {
        SelectedUnit = selectedUnit;
        MoveTaken = moveTaken;
    }

    public override void Enter( BattleSystem sys )
    {
        Button defaultSelected = null;
        Button waitButton = sys.Menu.AddButton( "Wait", Wait );

        if ( MoveTaken == false )
            defaultSelected = sys.Menu.AddButton( "Move", StartMoving );

        defaultSelected = defaultSelected == null ? waitButton : defaultSelected;

        var interactables = GetAttackableUnits( unit => unit.tag == "Player" );
        if ( interactables.Count() > 0 )
            defaultSelected = sys.Menu.AddButton( "Attack", () => StartAttacking( interactables ) );

        EventSystem.current.SetSelectedGameObject( defaultSelected.gameObject );
    }

    private void StartMoving()
    {
        sys.TurnState = WhereToMoveState.Create( SelectedUnit );
        SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Selected", false );
    }

    private IEnumerable<Unit> GetAttackableUnits( Predicate<Unit> unitPredicate )
    {
        foreach ( var tile in sys.Map.GetTilesWithinAbsoluteRange( SelectedTile.Position, SelectedUnit.AttackRange ) )
        {
            //You can't interact with yourself =)
            if ( tile == SelectedTile.Position )
                continue;

            Unit unitToCheck = null;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Map[ tile ], out unitToCheck ) )
            {
                if ( unitPredicate( unitToCheck ) )
                    break;
                else
                    yield return unitToCheck;
            }
        }
        yield break;
    }

    private void Wait()
    {
        sys.CurrentTurn.GoToStateAndForget( ChoosingUnitState.Instance );
        sys.CurrentTurn.UnitFinished( SelectedUnit );
    }

    private void StartAttacking( IEnumerable<Unit> interactables )
    {
        sys.TurnState = ChooseAttacksState.Create( SelectedUnit, interactables );
    }
}

