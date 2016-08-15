using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.General.DataStructures;
using UnityEngine.EventSystems;

namespace Assets.Map
{
    public class BattleSystem : MonoBehaviour
    {
        private enum BattleTurn
        {
            Player,
            Enemy
        };

        public GameMap Map;
        public Animator MenuAnimator;
        public CursorControl Cursor;
        public GameObject FirstSelectedMenuButton;
        private EnemyState _EnemyState;
        private BattleTurn _Turn;

        void Awake()
        {
            BattleState.CurrentState = new PlayerSelectingUnit();
            BattleState.currentBattleSystem = this;
        }

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            switch ( _Turn )
            {
                case BattleTurn.Player:
                    BattleState.CurrentState.Update( BattleState.CurrentState );
                    break;
                case BattleTurn.Enemy:
                    _EnemyState.Update();
                    break;

            }
        }

        public void HandleMessage( string selection )
        {
            BattleState.CurrentState.HandleMessage( selection );
        }
    }

    public class EnemyState
    {
        public void Update() { }
    }

    public interface IPlayerState
    {
        void Update( IPlayerState state );
        void HandleMessage( string message );
        void Enter( IPlayerState state );
        void Exit( IPlayerState state );
    }

    public abstract class BattleState : IPlayerState
    {
        public const string MoveMessage = "Move";
        public const string WaitMessage = "Wait";

        private static Stack<IPlayerState> _CurrentStateStack = new Stack<IPlayerState>( new IPlayerState[] { new NullState() } );
        public static IPlayerState CurrentState {
            get { return _CurrentStateStack.Peek(); }
            set
            {
                _CurrentStateStack.Peek().Exit( value );
                value.Enter( _CurrentStateStack.Peek() );
                _CurrentStateStack.Push( value );
            }
        }

        public static void RollBackToPreviousState()
        {
            IPlayerState poppedState = _CurrentStateStack.Pop();
            poppedState.Exit( CurrentState );
            CurrentState.Enter( poppedState );
        }

        public static BattleSystem currentBattleSystem;

        //public static GameMap Map;
        //public static CursorControl CursorControl;
        //public static Animator MenuAnimator;

        public abstract void HandleMessage( string message );

        public abstract void Update( IPlayerState state );

        public abstract void Enter( IPlayerState state );

        public abstract void Exit( IPlayerState state );
    }

    //State that does nothing
    public class NullState : IPlayerState
    {
        public void Enter( IPlayerState state )
        {
        }

        public void Exit( IPlayerState state )
        {
        }

        public void HandleMessage( string message )
        {
        }

        public void Update( IPlayerState state )
        {
        }
    }

    public class Debouncer 
    {
        private float timeSince;
        private float delay;

        public Action DelayedFunction;

        public Debouncer( Action function, float delay )
        {
            this.delay = delay;
            this.DelayedFunction = function;
        }

        public void Execute()
        {
            if ( timeSince < 0.0f )
            {
                DelayedFunction();
                timeSince = 0.15f;
            }
            else
                timeSince -= Time.deltaTime; 
        }
    }

    public class PlayerSelectingUnit : BattleState
    {
        public static Debouncer ShiftCursor = new Debouncer(
            delegate 
            {
                var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
                if ( direction.x != 0 || direction.y != 0 )
                {
                    currentBattleSystem.Cursor.ShiftCursor( direction );
                }
            }, 0.12f );

        public override void Update( IPlayerState currentState )
        {
            ShiftCursor.Execute();

            var tile = currentBattleSystem.Cursor.CurrentTile;
            if ( tile != null )
            {
                if ( tile.UnitOccupying != null )
                {
                    currentBattleSystem.Map.RenderUnitMovement( tile.UnitOccupying, 0.5f );
                    if ( Input.GetButtonDown( "Jump" ) )
                    {
                        CurrentState = new PlayerMenuSelection( tile.UnitOccupying );
                    }
                }
                else
                    currentBattleSystem.Map.StopRenderingOverlays();
            }
        }

        public override void HandleMessage( string message ) { }

        public override void Enter( IPlayerState state ) { } 

        public override void Exit( IPlayerState state ) { }
    }

    public class PlayerSelectingForAttacks : PlayerSelectingUnit
    {
        private Unit _SelectedUnit;

        public PlayerSelectingForAttacks( Unit selectedUnit )
        {
            _SelectedUnit = selectedUnit;
        }

        public override void Update( IPlayerState state )
        {
            currentBattleSystem.Map.RenderUnitMovement( _SelectedUnit );

            ShiftCursor.Execute();

            if ( Input.GetButtonDown( "Cancel" ) )
                RollBackToPreviousState();
        }

        public override void HandleMessage( string message ) { }

        public override void Enter( IPlayerState state ) { }

        public override void Exit( IPlayerState state )
        {
            currentBattleSystem.Cursor.MoveCursor( _SelectedUnit.Position );
        }
    }

    public class PlayerMenuSelection : BattleState
    {
        public Unit SelectedUnit;

        public PlayerMenuSelection( Unit unit)
        {
            SelectedUnit = unit;
        }

        public override void Update( IPlayerState state )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
            {
                RollBackToPreviousState();
            }
        }

        public override void HandleMessage( string message )
        {
            Debug.Log( string.Format( "This is from: {0}, The Message: {1}", this, message ) );
            switch ( message )
            {
                case WaitMessage:
                    RollBackToPreviousState();
                    break;
                case MoveMessage:
                    CurrentState = new PlayerSelectingForAttacks( SelectedUnit );
                    break;
            }
        }

        public override void Enter( IPlayerState state )
        {
            currentBattleSystem.MenuAnimator.SetBool( "Hidden", false );
            EventSystem.current.SetSelectedGameObject( currentBattleSystem.FirstSelectedMenuButton );
        }

        public override void Exit( IPlayerState state )
        {
            currentBattleSystem.MenuAnimator.SetBool( "Hidden", true );
        }
    }
}