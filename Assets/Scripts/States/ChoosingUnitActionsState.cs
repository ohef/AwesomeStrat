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

    public ChoosingUnitActionsState( Unit selectedUnit, bool moveTaken = false )
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

        var interactables = GetAttackableUnits();
        if ( interactables.Count() > 0 )
            defaultSelected = sys.Menu.AddButton( "Attack", () => StartChooseAttacks( interactables ) );

        EventSystem.current.SetSelectedGameObject( defaultSelected.gameObject );
    }

    private void StartMoving()
    {
        sys.TurnState = new WhereToMoveState( SelectedUnit );
        SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Selected", false );
    }

    private IEnumerable<Unit> GetAttackableUnits()
    {
        foreach ( var tile in sys.Map.GetTilesWithinAbsoluteRange( SelectedTile.Position, SelectedUnit.AttackRange ) )
        {
            if ( tile == SelectedTile.Position )
                continue;

            Unit unitCheck = null;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Map[ tile ], out unitCheck ) )
                yield return unitCheck;
        }
        yield break;
    }

    private void Wait()
    {
        sys.CurrentTurn.GoToStateAndForget( ChoosingUnitState.Instance );
        sys.CurrentTurn.UnitFinished( SelectedUnit );
    }

    private void StartChooseAttacks( IEnumerable<Unit> interactables )
    {
        sys.TurnState = new ChooseAttacksState( SelectedUnit, interactables.Where( i => i is Unit ).Cast<Unit>() );
    }
}

