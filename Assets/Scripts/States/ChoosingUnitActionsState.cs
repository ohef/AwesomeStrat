using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosingUnitActionsState : MenuState
{
    private MapUnit SelectedUnit;
    private GameTile SelectedTile { get { return sys.Map.UnitGametileMap[ SelectedUnit ]; } }

    public ChoosingUnitActionsState( MapUnit selectedUnit )
    {
        SelectedUnit = selectedUnit;
    }

    public override void Enter( BattleSystem sys )
    {
        Button defaultSelected = sys.Menu.AddButton( "Wait", Wait );

        if ( SelectedUnit.hasMoved == false )
            sys.Menu.AddButton( "Move", StartMoving );

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

    private IEnumerable<MapUnit> GetAttackableUnits()
    {
        foreach ( var tile in sys.Map.GetTilesWithinAbsoluteRange( SelectedTile.Position, SelectedUnit.AttackRange ) )
        {
            if ( tile == SelectedTile.Position )
                continue;

            MapUnit unitCheck = null;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Map[ tile ], out unitCheck ) )
                yield return unitCheck;
        }
        yield break;
    }

    private void Wait()
    {
        sys.CurrentTurn.GoToStateAndForget( ChoosingUnitState.Instance );
        SelectedUnit.hasTakenAction = true;
    }

    private void StartChooseAttacks( IEnumerable<MapUnit> interactables )
    {
        sys.TurnState = new ChooseAttacksState( SelectedUnit, interactables.Where( i => i is MapUnit ).Cast<MapUnit>() );
    }
}

