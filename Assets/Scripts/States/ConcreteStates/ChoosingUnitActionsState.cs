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

    public class StateForAbility : IAbilityGeneric<object>
    {
        public ChoosingUnitActionsState Container;
        public PlayerTurnController Context;

        public object Visit( WaitAbility ability )
        {
            Context.GoToStateAndForget( ChoosingUnitState.Instance );
            Context.UnitFinished( Container.SelectedUnit );
            return null;
        }

        public object Visit( AreaOfEffectAbility ability )
        {
            return null;
        }

        public object Visit( TargetAbility ability )
        {
            var map = BattleSystem.Instance.Map;
            var unitPos = map.UnitGametileMap[ Container.SelectedUnit ].Position;
            if ( ability.GetInteractableUnits(
                 map.GetUnitsWithinRange( unitPos, ability.Range ),
                 ability.GetTargetPredicate( Context ) )
                .Count() > 0 )
                Context.State = ChooseTargetsState.Create( ability );
            return null;
        }
    }

    public static BattleState Create( Unit selectedUnit )
    {
        return new CancelableState( new ChoosingUnitActionsState( selectedUnit ) );
    }

    private ChoosingUnitActionsState( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
    }

    public override void Enter( PlayerTurnController context )
    {
        var visitor = new StateForAbility { Container = this, Context = context };
        List<Button> buttons = GetButtons( SelectedUnit.Abilities, visitor ).ToList();
        sys.Menu.AddButtons( buttons );
        EventSystem.current.SetSelectedGameObject( buttons.First().gameObject );
    }

    private IEnumerable<Button> GetButtons( IEnumerable<Ability> abilities, StateForAbility visitor )
    {
        foreach ( Ability ability in abilities )
        {
            Button button = AbilityButtonFactory.instance.Create( ability );
            button.onClick.AddListener( () => ability.Accept( visitor ) );
            yield return button;
        }
    }
}