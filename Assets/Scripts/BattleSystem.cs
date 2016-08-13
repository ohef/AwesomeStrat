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
            BattleState.CurrentState = BattleState.playerSelectingUnit = new PlayerSelectingUnit();
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

        private static IPlayerState _CurrentState = new PlayerSelectingUnit();
        private static Stack<IPlayerState> _CurrentStateStack = new Stack<IPlayerState>();
        public static IPlayerState CurrentState {
            get { return _CurrentState; }
            set
            {
                //_CurrentStateStack.Peek().Exit( value );
                _CurrentState.Exit( value );
                value.Enter( _CurrentState );
                _CurrentState = value;
            }
        }

        public static void RollBackToPreviousState()
        {

        }

        public static GameMap Map;
        public static PlayerSelectingUnit playerSelectingUnit;
        public static CursorControl CursorControl;
        public static Animator MenuAnimator;

        public BattleState() { }

        public abstract void HandleMessage( string message );

        public abstract void Update( IPlayerState state );

        public abstract void Enter( IPlayerState state );

        public abstract void Exit( IPlayerState state );
    }

    public class PlayerSelectingUnit : BattleState
    {
        public override void Update( IPlayerState currentState )
        {
            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if ( direction.x != 0 || direction.y != 0 )
                CursorControl.MoveCursor( direction );

            var tile = CursorControl.CurrentTile;
            if ( tile != null )
            {
                if ( tile.UnitOccupying != null )
                {
                    Map.RenderUnitMovement( tile.UnitOccupying, 0.5f );
                    if ( Input.GetButtonDown( "Jump" ) )
                    {
                        CurrentState = new PlayerMenuSelection( this, tile.UnitOccupying );
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
                CursorControl.MoveCursor( direction );

            if ( Input.GetButtonDown( "Cancel" ) )
            {
                CurrentState = playerSelectingUnit;
            }
        }

        public override void HandleMessage( string message )
        {
        }

        public override void Enter( IPlayerState state )
        {
            MenuAnimator.SetBool( "Hidden", true );
        }

        public override void Exit( IPlayerState state )
        {
            MenuAnimator.SetBool( "Hidden", false );
        }
    }

    public class PlayerMenuSelection : BattleState
    {
        public IPlayerState PreviousState;
        public Unit SelectedUnit;

        public PlayerMenuSelection( IPlayerState currentState, Unit unit)
        {
            PreviousState = currentState;
            SelectedUnit = unit;
        }

        public override void Update( IPlayerState state )
        {
            if ( Input.GetButton( "Cancel" ) )
            {
                CurrentState =  PreviousState;
            }
        }

        public override void HandleMessage( string message )
        {
            Debug.Log( string.Format( "This is from: {0}, The Message: {1}", this, message ) );
            switch ( message )
            {
                case WaitMessage:
                    CurrentState = PreviousState;
                    break;
                case MoveMessage:
                    CurrentState = new PlayerSelectingForAttacks( SelectedUnit );
                    break;
                default:
                    CurrentState = this;
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