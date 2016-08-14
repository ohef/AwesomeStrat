using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.General.DataStructures;
using Assets.Map;

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
        public GameObject UnitMenu;
        private EnemyState _EnemyState;
        private BattleTurn _Turn;

        void Awake()
        {
            BattleState.Map = Map;
            BattleState.CursorControl = Map.GetComponentInChildren<CursorControl>();
            BattleState.CurrentState = new PlayerSelectingUnit();
            BattleState.MenuAnimator = UnitMenu.GetComponent<Animator>();
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

        public static GameMap Map;
        public static CursorControl CursorControl;
        public static Animator MenuAnimator;

        public BattleState() { }

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

    public class PlayerSelectingUnit : BattleState
    {
        public override void Update( IPlayerState currentState )
        {
            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if ( direction.x != 0 || direction.y != 0 )
                CursorControl.ShiftCursor( direction );

            var tile = CursorControl.CurrentTile;
            if ( tile != null )
            {
                if ( tile.UnitOccupying != null )
                {
                    Map.RenderUnitMovement( tile.UnitOccupying, 0.5f );
                    if ( Input.GetButtonDown( "Jump" ) )
                    {
                        CurrentState = new PlayerMenuSelection( tile.UnitOccupying );
                    }
                }
                else
                    Map.StopRenderingOverlays();
            }
        }

        public override void HandleMessage( string message )
        {
        }

        public override void Enter( IPlayerState state ) { } 

        public override void Exit( IPlayerState state ) { }
    }

    public class PlayerSelectingForAttacks : BattleState
    {
        private Unit _SelectedUnit;

        public PlayerSelectingForAttacks( Unit selectedUnit )
        {
            _SelectedUnit = selectedUnit;
        }

        public override void Update( IPlayerState state )
        {
            Map.RenderUnitMovement( _SelectedUnit );

            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if ( direction.x != 0 || direction.y != 0 )
                CursorControl.ShiftCursor( direction );

            if ( Input.GetButtonDown( "Cancel" ) )
            {
                RollBackToPreviousState();
            }
        }

        public override void HandleMessage( string message )
        {
        }

        public override void Enter( IPlayerState state ) { }

        public override void Exit( IPlayerState state )
        {
            CursorControl.MoveCursor( _SelectedUnit.Position );
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
                default:
                    break;
            }
        }

        public override void Enter( IPlayerState state )
        {
            MenuAnimator.SetBool( "Hidden", false );
        }

        public override void Exit( IPlayerState state )
        {
            MenuAnimator.SetBool( "Hidden", true );
        }
    }
}