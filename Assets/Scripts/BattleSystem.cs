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

        void Awake()
        {
            instance = this;
        }

        // Use this for initialization
        void Start()
        {
            BattleState.CurrentState = new PlayerSelectingUnit();
            Cursor.CursorMoved += CursorMoved;
            EventSystem.current.SetSelectedGameObject( Cursor.gameObject );
        }

        private void CursorMoved( GameTile tile )
        {
            if ( BattleState.CurrentState is PlayerSelectingUnit )
            {
                Map.ShowUnitMovementIfHere( tile );
            }
        }

        // Update is called once per frame
        void Update()
        {
            BattleState.CurrentState.Update( BattleState.CurrentState );
        }
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

            sys.Cursor.UpdateAction();

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
                    CurrentState = new PlayerMenuSelection( unitAtTile );
                }
            }
        }

        public override void Enter( IPlayerState state ) {
        } 

        public override void Exit( IPlayerState state ) {
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

    public class PlayerUnitAction : BattleState
    {
        private Unit SelectedUnit;
        private GameTile UnitTile;
        private HashSet<Vector2Int> MovementTiles;
        private HashSet<Vector2Int> AttackTiles;

        public PlayerUnitAction( Unit selectedUnit )
        {
            SelectedUnit = selectedUnit;
            UnitTile = sys.Map.UnitGametileMap[ SelectedUnit ];
            MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, UnitTile ) );
            AttackTiles = sys.Map.GetAttackTiles( MovementTiles, SelectedUnit.AttackRange );
        }

        public override void Update( IPlayerState state )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
                GoToPreviousState();

            {
                sys.Cursor.UpdateAction();

                bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
                bool canAttackHere = AttackTiles.Contains( sys.Cursor.CurrentTile.Position );
                Unit unitUnderCursor = null;
                sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitUnderCursor );

                if ( Input.GetButtonDown( "Submit" ) )
                {
                    bool notTheSameUnit = unitUnderCursor != SelectedUnit;
                    if ( canMoveHere && notTheSameUnit )
                    {
                        ExecuteMove();
                    }
                    else
                    {
                        if ( unitUnderCursor != null && notTheSameUnit && canAttackHere )
                            ExecuteAttack( SelectedUnit, unitUnderCursor );
                        else
                            return;
                    }

                    sys.Map.ShowUnitMovement( SelectedUnit );
                    GoToPreviousState();
                    GoToPreviousState();
                }
            }
        }

        private void ExecuteAttack( Unit selectedUnit, Unit unitUnderCursor )
        {
            GameTile optimalAttackPos = GetOptimalAttackPosition( sys.Cursor.CurrentTile );
            List<GameTile> optimalPath = MapSearcher.Search(
                UnitTile,
                optimalAttackPos,
                sys.Map,
                SelectedUnit.MovementRange );

            if ( optimalPath != null )
                sys.StartCoroutine(
                    General.CustomAnimation.InterpolateBetweenPoints(
                        SelectedUnit.transform,
                        optimalPath.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(),
                        0.11f ) );

            sys.Map.SwapUnit( UnitTile, optimalAttackPos );

            unitUnderCursor.HP -= selectedUnit.Attack - unitUnderCursor.Defense;
        }

        private GameTile GetOptimalAttackPosition( GameTile from )
        {
            // Does a reverse lookup from position to see if there are any 
            // tiles we can move around the point we can move to
            IEnumerable<Vector2Int> canMovePositions = sys.Map
                .GetTilesWithinAbsoluteRange( from.Position, SelectedUnit.AttackRange )
                .Where( pos => MovementTiles.Contains( pos ) );

            if ( canMovePositions.Count() == 0 )
                return null;

            Vector2Int optimalPosition = canMovePositions
                .Select( pos => new { Pos = pos, Distance = from.Position.ManhattanDistance( pos ) } )
                .OrderByDescending( data => data.Distance )
                .First()
                .Pos;

            //Vector2Int optimalPosition = from.Position;
            return sys.Map[ optimalPosition ];
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
        }

        public override void Exit( IPlayerState state )
        {
            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        }
    }
}