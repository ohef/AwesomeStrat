using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosingUnitActionsState : MenuState
{
    public class StateForAbility : IAbilityVisitor<object>
    {
        public ChoosingUnitActionsState Container;
        public PlayerTurnController Context;

        public object Visit( WaitAbility ability )
        {
            Context.GoToStateAndForget( BattleSystem.Instance.GetState<ChoosingUnitState>() );
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
            var unitPos = map.UnitPos[ Container.SelectedUnit ];
            var targetableUnits = map.GetUnitsWithinRange( unitPos, ability.Range )
                .Where( ability.CanTargetFunction( Container.SelectedUnit, Context ) );

            if ( targetableUnits.Count() > 0 )
            {
                ChooseTargetsState state = BattleSystem.Instance.GetState<ChooseTargetsState>();
                state.Initialize( ability, Container.SelectedUnit );
                Context.State = state;
            }
            return null;
        }
    }

    private Unit SelectedUnit;

    public void Initialize( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
    }

    public void OnEnable()
    {
        var context = sys.CurrentTurn as PlayerTurnController;
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