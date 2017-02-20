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

        private Stack<IPlayerState> _CurrentStateStack = new Stack<IPlayerState>( new IPlayerState[] { new NullState() } );
        public IPlayerState CurrentState {
            get { return _CurrentStateStack.Peek(); }
            set
            {
                _CurrentStateStack.Peek().Exit( this );
                value.Enter( this );
                _CurrentStateStack.Push( value );
            }
        }

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
            CurrentState = new PlayerSelectingUnit();
            Cursor.CursorMoved += CursorMoved;
            EventSystem.current.SetSelectedGameObject( Cursor.gameObject );
        }

        private void CursorMoved( GameTile tile )
        {
            if ( CurrentState is PlayerSelectingUnit )
            {
                Map.ShowUnitMovementIfHere( tile );
            }
        }

        // Update is called once per frame
        void Update()
        {
            CurrentState.Update( this );
        }

        public void GoToPreviousState()
        {
            IPlayerState poppedState = _CurrentStateStack.Pop();
            poppedState.Exit( this );
            CurrentState.Enter( this );
        }
    }

    public interface IPlayerState
    {
        void Update( BattleSystem state );
        void Enter( BattleSystem state );
        void Exit( BattleSystem state );
    }

    public abstract class BattleState : IPlayerState
    {
        /// <summary>
        //TODO is this really needed?
        /// </summary>
        protected BattleSystem sys { get { return BattleSystem.instance; } }

        public abstract void Update( BattleSystem sys );

        public abstract void Enter( BattleSystem sys );

        public abstract void Exit( BattleSystem sys );
    }

    public class NullState : IPlayerState
    {
        public void Enter( BattleSystem state )
        {
        }

        public void Exit( BattleSystem state )
        {
        }

        public void Update( BattleSystem state )
        {
        }
    }

    public class PlayerSelectingUnit : BattleState
    {
        public override void Update( BattleSystem sys )
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
                    sys.CurrentState = new PlayerMenuSelection { SelectedUnit = unitAtTile };
                }
            }
        }

        public override void Enter( BattleSystem sys ) { }

        public override void Exit( BattleSystem sys ) { }
    }

    public class PlayerMenuSelection : BattleState
    {
        public Unit SelectedUnit;
        public Button MoveButton;

        public override void Update( BattleSystem sys )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
            {
                sys.GoToPreviousState();
            }
        }

        public override void Enter( BattleSystem sys )
        {
            GameObject menu = GameObject.Find( "Menu" );
            MoveButton = GameObject.Instantiate( sys.MenuButton );
            MoveButton.transform.SetParent( menu.transform );
            MoveButton.GetComponentInChildren<Text>().text = "Move";
            MoveButton.GetComponent<Button>().onClick.AddListener( StartMoving );
            EventSystem.current.SetSelectedGameObject( MoveButton.gameObject );
        }

        private void StartMoving()
        {
            sys.CurrentState = new PlayerUnitAction
            {
                SelectedUnit = SelectedUnit,
                UnitTile = sys.Map.UnitGametileMap[ SelectedUnit ]
            };
        }


        public override void Exit( BattleSystem sys )
        {
            Input.ResetInputAxes();

            if ( MoveButton.IsDestroyed() == false )
            {
                GameObject.Destroy( MoveButton.gameObject );
            }
        }
    }

    public class PlayerUnitAction : BattleState
    {
        public Unit SelectedUnit;
        public GameTile UnitTile;
        public HashSet<Vector2Int> MovementTiles;
        public HashSet<Vector2Int> AttackTiles;

        private void ExecuteAttack( Unit selectedUnit, Unit unitUnderCursor )
        {
            GameTile optimalAttackPos = GetOptimalAttackPosition( sys.Cursor.CurrentTile );
            if ( optimalAttackPos != UnitTile ) //We need to move
            {
                ExecuteMove( optimalAttackPos );
            }
            unitUnderCursor.HP -= selectedUnit.Attack - unitUnderCursor.Defense;
        }

        private GameTile GetOptimalAttackPosition( GameTile on )
        {
            // Does a reverse lookup from position to see if there are any 
            // tiles we can move around the point we can move to
            IEnumerable<Vector2Int> canMovePositions = sys.Map
                .GetTilesWithinAbsoluteRange( on.Position, SelectedUnit.AttackRange )
                .Where( pos => MovementTiles.Contains( pos ) )
                .ToList();

            if ( canMovePositions.Count() == 0 )
                return null;
            else if ( canMovePositions.Any( pos => pos == UnitTile.Position ) )
                return UnitTile;

            Vector2Int optimalPosition = canMovePositions
                .Select( pos => new { Pos = pos, Distance = on.Position.ManhattanDistance( pos ) } )
                .OrderByDescending( data => data.Distance )
                .First()
                .Pos;

            return sys.Map[ optimalPosition ];
        }

        private void ExecuteMove( GameTile to )
        {
            List<GameTile> optimalPath = MapSearcher.Search(
                UnitTile,
                to,
                sys.Map,
                SelectedUnit.MovementRange );

            sys.StartCoroutine(
                General.CustomAnimation.InterpolateBetweenPoints(
                    SelectedUnit.transform,
                    optimalPath.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(),
                    0.11f ) );

            sys.Map.SwapUnit( UnitTile, to);
        }

        public override void Update( BattleSystem sys )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
                sys.GoToPreviousState();

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
                        ExecuteMove( sys.Cursor.CurrentTile );
                    }
                    else
                    {
                        if ( unitUnderCursor != null && notTheSameUnit && canAttackHere )
                            ExecuteAttack( SelectedUnit, unitUnderCursor );
                        else
                            return;
                    }

                    sys.Map.ShowUnitMovement( SelectedUnit );
                    sys.GoToPreviousState();
                    sys.GoToPreviousState();
                }
            }
        }

        public override void Enter( BattleSystem sys )
        {
            MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, UnitTile ) );
            AttackTiles = sys.Map.GetAttackTiles( MovementTiles, SelectedUnit.AttackRange );
        }

        public override void Exit( BattleSystem sys )
        {
            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        }
    }
}