using UnityEngine;
using System.Collections;
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
        private BattleState _PlayerState;
        private EnemyState _EnemyState;
        private BattleTurn _Turn;

        void Awake()
        {
            BattleState.Map = Map;
            BattleState.CursorControl = Map.GetComponentInChildren<CursorControl>();
            _PlayerState = BattleState.playerSelectingUnit = new PlayerSelectingUnit();
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
                    _PlayerState.Update( _PlayerState );
                    break;
                case BattleTurn.Enemy:
                    _EnemyState.Update();
                    break;

            }
        }

        public void HandleMessage( string selection )
        {
            _PlayerState.HandleMessage( selection );
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
    }

    public abstract class BattleState : IPlayerState
    {
        public const string MoveMessage = "Move";
        public const string WaitMessage = "Wait";

        public static IPlayerState CurrentState;
        public static GameMap Map;
        public static PlayerSelectingUnit playerSelectingUnit;
        public static CursorControl CursorControl;

        public BattleState() { }

        public abstract void HandleMessage( string message );

        public abstract void Update( IPlayerState state );
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
    }

    public class PlayerSelectingForAttacks : BattleState
    {
        private Unit _SelectedUnit;

        public PlayerSelectingForAttacks(  Unit selectedUnit )
        {
            _SelectedUnit = selectedUnit;
        }

        public override void Update( IPlayerState state )
        {
            Map.RenderUnitMovement( _SelectedUnit );

            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if ( direction.x != 0 || direction.y != 0 )
                CursorControl.MoveCursor( direction );

            if ( Input.GetButtonDown( "Jump" ) )
            {
                CurrentState = playerSelectingUnit;
            }
        }

        public override void HandleMessage( string message )
        {
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
                    CurrentState = new PlayerSelectingForAttacks(  SelectedUnit );
                    break;
                default:
                    CurrentState = this;
                    break;
            }
        }
    }
}