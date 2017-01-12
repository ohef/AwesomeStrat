﻿using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.General.DataStructures;
using UnityEngine.EventSystems;
using System.Linq;

namespace Assets.Map
{
    public class BattleSystem : MonoBehaviour
    {
        private List<BattleState> PlayerTurns;
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
            BattleState.sys = this;
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

        public static BattleSystem sys;

        public static void RollBackToPreviousState()
        {
            IPlayerState poppedState = _CurrentStateStack.Pop();
            poppedState.Exit( CurrentState );
            CurrentState.Enter( poppedState );
        }

        protected static Vector2Int UpdateCursor()
        {
            int vertical = ( Input.GetButtonDown( "Up" ) ? 1 : 0 ) + ( Input.GetButtonDown( "Down" ) ? -1 : 0 );
            int horizontal = ( Input.GetButtonDown( "Left" ) ? -1 : 0 ) + ( Input.GetButtonDown( "Right" ) ? 1 : 0 );
            var inputVector = new Vector2Int( horizontal, vertical );
            if ( vertical != 0 || horizontal != 0 )
                return sys.Cursor.ShiftCursor( inputVector );

            return Vector2Int.Zero;
        }

        public abstract void HandleMessage( string message );

        public abstract void Update( IPlayerState state );

        public abstract void Enter( IPlayerState state );

        public abstract void Exit( IPlayerState state );
    }

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
            UpdateCursor();
            var tile = sys.Cursor.CurrentTile;
            if ( tile != null && tile.IsUnitPresent == true )
            {
                sys.Map.RenderUnitMovement( tile.Unit, 0.5f );
                if ( Input.GetButtonDown( "Jump" ) )
                {
                    CurrentState = new PlayerMenuSelection( tile.Unit );
                }
            }
            else
                sys.Map.StopRenderingOverlays();
        }

        public override void HandleMessage( string message ) { }

        public override void Enter( IPlayerState state ) { } 

        public override void Exit( IPlayerState state ) { }
    }

    public class PlayerUnitAction : PlayerSelectingUnit
    {
        private Unit _SelectedUnit;
        private HashSet<Vector2Int> movementTiles;

        public PlayerUnitAction( Unit selectedUnit )
        {
            _SelectedUnit = selectedUnit;
            movementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( _SelectedUnit ) );
            movementTiles.Remove( sys.Map[ _SelectedUnit ].Position );
        }

        public override void Update( IPlayerState state )
        {
            //TODO: Wayyyyyyyy too deep on member accessors, find a better way.
            sys.Map.RenderUnitMovement( _SelectedUnit );
            UpdateCursor();

            if ( Input.GetButtonDown( "Cancel" ) )
                RollBackToPreviousState();

            if ( Input.GetButtonDown( "Submit" ) 
                && !sys.Cursor.CurrentTile.Equals( sys.Map[ _SelectedUnit ] )
                && movementTiles.Contains( sys.Cursor.CurrentTile.Position ) )
            {
                List<GameTile> optimalPath = MapSearcher.Search(
                    sys.Map[ _SelectedUnit ],
                    sys.Cursor.CurrentTile,
                    sys.Map.m_TileMap,
                    _SelectedUnit.Movement );

                sys.StartCoroutine(
                    General.CustomAnimation.InterpolateBetweenPoints(
                        _SelectedUnit.transform,
                        optimalPath.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(),
                        0.11f ) );

                sys.Map.SwapUnit( sys.Map[ _SelectedUnit ], sys.Cursor.CurrentTile );

                RollBackToPreviousState();
                RollBackToPreviousState();
            }
        }

        public override void HandleMessage( string message ) { }

        public override void Enter( IPlayerState state ) { }

        public override void Exit( IPlayerState state )
        {
            sys.Cursor.MoveCursor( sys.Map[ _SelectedUnit ].Position );
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
                    CurrentState = new PlayerUnitAction( SelectedUnit );
                    break;
            }
        }

        public override void Enter( IPlayerState state )
        {
            sys.MenuAnimator.SetBool( "Hidden", false );
            EventSystem.current.SetSelectedGameObject( sys.FirstSelectedMenuButton );
        }

        public override void Exit( IPlayerState state )
        {
            sys.MenuAnimator.SetBool( "Hidden", true );
        }
    }
}