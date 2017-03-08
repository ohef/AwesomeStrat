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

    public static BattleState Create( Unit selectedUnit)
    {
        return new CancelableState( new ChoosingUnitActionsState( selectedUnit ) );
    }

    private ChoosingUnitActionsState( Unit selectedUnit)
    {
        SelectedUnit = selectedUnit;
    }

    public override void Enter( BattleSystem sys )
    {
        Button defaultSelected = null;
        Button waitButton = sys.Menu.AddButton( "Wait", Wait );

        defaultSelected = defaultSelected == null ? waitButton : defaultSelected;

        var interactables = GetAttackableUnits( unit => unit.tag == "Player" );
        if ( interactables.Count() > 0 )
            defaultSelected = sys.Menu.AddButton( "Attack", () => StartAttacking( interactables ) );

        EventSystem.current.SetSelectedGameObject( defaultSelected.gameObject );
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
                    continue;
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

