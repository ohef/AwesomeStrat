using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Assets.General.DataStructures;
using System.Linq;
using UnityEngine.EventSystems;

namespace Assets.Map
{
    public class BattleSystem : MonoBehaviour
    {
        public static BattleSystem instance;
        private List<BattleState> PlayerTurns;
        private enum BattleTurn
        {
            Player,
            Enemy
        };

        public GameMap Map;
        public CursorControl Cursor;
        public Button MenuButton;
        public Canvas TempGFXCanvas;
        private EnemyState _EnemyState;
        private BattleTurn _Turn;

        void Awake()
        {
            instance = this;
        }

        // Use this for initialization
        void Start()
        {
            BattleState.CurrentState = new PlayerSelectingUnit();
            Cursor.CursorMoved += Map.ShowUnitMovementIfHere;
            EventSystem.current.SetSelectedGameObject( Cursor.gameObject );
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
    }

    public class EnemyState
    {
        public void Update() { }
    }

    public interface IPlayerState
    {
        void Update( IPlayerState state );
        void Enter( IPlayerState state );
        void Exit( IPlayerState state );
    }

    public abstract class BattleState : IPlayerState
    {
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

        public static void GoToPreviousState()
        {
            IPlayerState poppedState = _CurrentStateStack.Pop();
            poppedState.Exit( CurrentState );
            CurrentState.Enter( poppedState );
        }

        protected static BattleSystem sys { get { return BattleSystem.instance; } }

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

        public void Update( IPlayerState state )
        {
        }
    }

    public class PlayerSelectingUnit : BattleState
    {
        public static Action StopRendering;
        public override void Update( IPlayerState currentState )
        {
            GameTile tile = sys.Cursor.CurrentTile;
            Unit unitAtTile;

            sys.Map.UnitGametileMap.TryGetValue( tile, out unitAtTile );
            if ( unitAtTile != null )
            {
                GameObject infoData = GameObject.Find( "InfoData" );
                Action<string, int> setLabel = ( label, val ) =>
                infoData.transform.Find( label ).GetComponent<Text>().text = val.ToString();

                setLabel( "HP", unitAtTile.HP );
                setLabel( "Move", unitAtTile.MovementRange );
                setLabel( "Attack", unitAtTile.Attack );
                setLabel( "Defense", unitAtTile.Defense );

                if ( Input.GetButtonDown( "Submit" ) )
                {
                    sys.Cursor.CursorMoved -= sys.Map.ShowUnitMovementIfHere;
                    CurrentState = new PlayerMenuSelection( unitAtTile );
                }
            }
        }

        public override void Enter( IPlayerState state )
        {
            sys.Cursor.MovementEnabled = true;
        } 

        public override void Exit( IPlayerState state ) {
            sys.Cursor.MovementEnabled = false;
        }
    }

    public class PlayerMenuSelection : BattleState
    {
        public Unit SelectedUnit;
        public Button MoveButton;
        public GameObject Menu;

        public PlayerMenuSelection( Unit unit )
        {
            SelectedUnit = unit;

            GameObject menu = GameObject.Find( "Menu" );
            MoveButton  = GameObject.Instantiate( sys.MenuButton );
            MoveButton.transform.SetParent( menu.transform );
            MoveButton.GetComponentInChildren<Text>().text = "Move";
            MoveButton.GetComponent<Button>().onClick.AddListener( StartMoving );
            EventSystem.current.SetSelectedGameObject( MoveButton.gameObject );
        }

        private void StartMoving()
        {
            sys.Cursor.CursorMoved -= sys.Map.ShowUnitMovementIfHere;
            CurrentState = new PlayerUnitAction( SelectedUnit );
        }

        public override void Update( IPlayerState state )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
            {
                GoToPreviousState();
            }
        }

        public override void Enter( IPlayerState state ) { }

        public override void Exit( IPlayerState state )
        {
            Input.ResetInputAxes();

            if ( MoveButton.IsDestroyed() == false )
                GameObject.Destroy( MoveButton.gameObject );
        }
    }

    public class PlayerUnitAction : PlayerSelectingUnit
    {
        private Unit SelectedUnit;
        private GameTile UnitTile;
        private HashSet<Vector2Int> movementTiles;

        public PlayerUnitAction( Unit selectedUnit )
        {
            SelectedUnit = selectedUnit;
            UnitTile = sys.Map.UnitGametileMap[ SelectedUnit ];
            movementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, UnitTile ) );
            movementTiles.Remove( UnitTile.Position );
        }

        public override void Update( IPlayerState state )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
                GoToPreviousState();

            {
                bool canMoveHere = movementTiles.Contains( sys.Cursor.CurrentTile.Position );

                if ( Input.GetButtonDown( "Submit" ) )
                {
                    if ( canMoveHere )
                    {
                        ExecuteMove();
                    }
                    else
                    {
                        Unit unitUnderCursor;
                        sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitUnderCursor );

                        if ( unitUnderCursor != null )
                            ExecuteAttack( SelectedUnit, unitUnderCursor );
                    }

                    sys.Map.ShowUnitMovement( null );
                    sys.Cursor.CursorMoved += sys.Map.ShowUnitMovementIfHere;
                    GoToPreviousState();
                    GoToPreviousState();
                }
            }
        }

        private void ExecuteAttack( Unit selectedUnit, Unit unitUnderCursor )
        {
            unitUnderCursor.HP -= CalculateDamage( selectedUnit, unitUnderCursor );
        }

        private int CalculateDamage( Unit selectedUnit, Unit unitUnderCursor )
        {
            return selectedUnit.Attack - unitUnderCursor.Defense;
        }

        private void ExecuteMove()
        {
            List<GameTile> optimalPath = MapSearcher.Search(
                UnitTile,
                sys.Cursor.CurrentTile,
                sys.Map,
                SelectedUnit.MovementRange );

            sys.StartCoroutine(
                General.CustomAnimation.InterpolateBetweenPoints(
                    SelectedUnit.transform,
                    optimalPath.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(),
                    0.11f ) );

            sys.Map.SwapUnit( UnitTile, sys.Cursor.CurrentTile );
        }

        public override void Enter( IPlayerState state )
        {
            sys.Cursor.MovementEnabled = true;
        }

        public override void Exit( IPlayerState state )
        {
            sys.Cursor.MovementEnabled = false;
            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        }
    }
}