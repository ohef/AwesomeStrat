using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

sealed class ChoosingUnitActionsState : MenuState, IPointerDownHandler
{
    private class StateForAbility : IAbilityVisitor<object>
    {
        public ChoosingUnitActionsState Container;
        public TurnController Context;

        public object Visit( WaitAbility ability )
        {
            BattleSystem.Instance.GoToState( BattleSystem.Instance.GetState<ChoosingUnitState>() );
            BattleSystem.Instance.UnitFinished( Container.SelectedUnit );
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
                Container.Transition<ChooseTargetsState>(
                    state => state.Initialize( ability, Container.SelectedUnit ) );
            }
            return null;
        }
    }

    public Unit SelectedUnit;

    public void Initialize( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
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

    public override void Enter()
    {
        var context = sys.CurrentTurnController;
        var visitor = new StateForAbility { Container = this, Context = context };
        List<Button> buttons = GetButtons( SelectedUnit.Abilities, visitor ).ToList();
        sys.Menu.AddButtons( buttons );
        base.Enter();
    }

    public void Update()
    {
        //How do we get the menu to position itself after animations are done?
        // TODO: Update functions suck
        var vector = Camera.main.WorldToScreenPoint( sys.Cursor.transform.position );
        sys.Menu.transform.position = vector;
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        var unit = eventData.pointerPressRaycast.gameObject.GetComponent<Unit>();
        if ( unit == null )
            ExecuteEvents.Execute( gameObject, eventData, ExecuteEvents.cancelHandler );
    }
}