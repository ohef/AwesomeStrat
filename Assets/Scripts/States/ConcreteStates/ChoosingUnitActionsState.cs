using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

sealed class ChoosingUnitActionsState : MenuState, IPointerDownHandler
{
    private class ActionForAbility : IAbilityVisitor<UnityAction>
    {
        public ChoosingUnitActionsState Container;
        public TurnController Context;

        public UnityAction Visit( WaitAbility ability )
        {
            return delegate
            {
                BattleSystem.Instance.GoToState( BattleSystem.Instance.GetState<ChoosingUnitState>() );
                BattleSystem.Instance.UnitFinished( Container.SelectedUnit );
            };
        }

        public UnityAction Visit( AreaOfEffectAbility ability )
        {
            return null;
        }

        public UnityAction Visit( TargetAbility ability )
        {
            var map = BattleSystem.Instance.Map;
            var unitPos = map.UnitPos[ Container.SelectedUnit ];
            var targetableUnits = map.GetUnitsWithinRange( unitPos, ability.Range )
                .Where( ability.CanTargetFunction( Container.SelectedUnit, Context ) );

            if ( targetableUnits.Count() > 0 )
                return delegate {
                    Container.Transition<ChooseTargetsState>(
                    state => state.Initialize( ability, Container.SelectedUnit ) );
                };
            else return null;
        }
    }

    public Unit SelectedUnit;

    public void Initialize( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
    }

    private IEnumerable<Button> GetButtons( IEnumerable<IAbility> abilities, ActionForAbility visitor )
    {
        foreach ( var result in abilities
            .Select( ability => new { Ability = ability, Callback = ability.Accept( visitor ) } )
            .Where( result => result.Callback != null ) )
        {
            Button button = AbilityButtonFactory.instance.Create( result.Ability );
            button.onClick.AddListener( result.Callback );
            yield return button;
        }
    }

    public override void Enter()
    {
        sys.Menu.AddButtons( GetButtons( 
            SelectedUnit.Abilities,
            new ActionForAbility { Container = this, Context = sys.CurrentTurnController } )
            .ToList() );
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