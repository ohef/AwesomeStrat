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
        private static BattleSystem instance;
        public static BattleSystem Instance { get { return instance; } }

        private enum BattleTurn
        {
            Player,
            Enemy
        };

        private Stack<BattleState> _CurrentStateStack = new Stack<BattleState>( new BattleState[] { new NullState() } );
        public BattleState CurrentState {
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
            Cursor.CursorMoved.AddListener( CursorMoved );
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

        private void CursorMoved()
        {
            CurrentState.CursorMoved();
        }

        private void OnRenderObject()
        {
                PlayerUnitAction state = 
                    BattleSystem.Instance.CurrentState is PlayerUnitAction ? 
                    ( PlayerUnitAction )BattleSystem.Instance.CurrentState :
                    null;
            if ( state != null )
            {
                state.OnRenderObject();
            }
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
        protected BattleSystem sys { get { return BattleSystem.Instance; } }

        public virtual void CursorMoved() { }

        public abstract void Update( BattleSystem sys );

        public abstract void Enter( BattleSystem sys );

        public abstract void Exit( BattleSystem sys );
    }

    public class NullState : BattleState
    {
        public override void Enter( BattleSystem state )
        {
        }

        public override void Exit( BattleSystem state )
        {
        }

        public override void Update( BattleSystem state )
        {
        }
    }

    public class PlayerSelectingUnit : BattleState
    {
        public override void Update( BattleSystem sys )
        {
            sys.Cursor.UpdateAction();
            Unit unitAtTile;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitAtTile ) )
            {
                if ( Input.GetButtonDown( "Submit" ) )
                {
                    sys.CurrentState = new PlayerMenuSelection( unitAtTile );
                }
            }
        }

        public override void Enter( BattleSystem sys ) { }

        public override void Exit( BattleSystem sys ) { }

        public override void CursorMoved()
        {
            sys.Map.ShowUnitMovementIfHere( sys.Cursor.CurrentTile );
        }
    }

    public class PlayerMenuSelection : BattleState
    {
        private Unit SelectedUnit;
        private Button MoveButton;

        public PlayerMenuSelection( Unit selectedUnit )
        {
            SelectedUnit = selectedUnit;
        }

        public override void Update( BattleSystem sys )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
            {
                sys.GoToPreviousState();
            }
        }

        public override void Enter( BattleSystem sys )
        {
            //TODO: Put this in a member var
            CommandMenu menu = GameObject.Find( "Menu" ).GetComponent<CommandMenu>();
            Button addedButton = menu.AddButton( "Move", StartMoving );
            EventSystem.current.SetSelectedGameObject( addedButton.gameObject );
        }

        public override void Exit( BattleSystem sys )
        {
            //To prevent the next state from catching the submit button
            Input.ResetInputAxes();

            CommandMenu menu = GameObject.Find( "Menu" ).GetComponent<CommandMenu>();
            menu.ClearButtons();
        }

        private void StartMoving()
        {
            sys.CurrentState = new PlayerUnitAction( SelectedUnit, sys.Map.UnitGametileMap[ SelectedUnit ] );
        }
    }

    public class PlayerUnitAction : BattleState
    {
        private Unit SelectedUnit;
        private GameTile UnitTile;
        private HashSet<Vector2Int> MovementTiles;
        private HashSet<Vector2Int> AttackTiles;
        private LinkedList<GameTile> TilesToPass ;

        public PlayerUnitAction(Unit selectedUnit, GameTile unitTile)
        {
            SelectedUnit = selectedUnit;
            UnitTile = unitTile;
            TilesToPass = new LinkedList<GameTile>();
            TilesToPass.AddFirst( UnitTile );
        }

        #region Interface Implementation
        public override void Update( BattleSystem sys )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
                sys.GoToPreviousState();

            sys.Cursor.UpdateAction();

            bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
            bool canAttackHere = AttackTiles.Contains( sys.Cursor.CurrentTile.Position );
            Unit unitUnderCursor = null;
            sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitUnderCursor );

            if ( Input.GetButtonDown( "Submit" ) )
            {
                //TODO: Panel System is needed
                //var toInteractWith = GetInteractableUnits( unitUnderCursor, sys.Cursor.CurrentTile );

                //if ( toInteractWith != null )
                //{

                //}

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

        private IEnumerable<Unit> GetInteractableUnits( Unit unitUnderCursor, GameTile currentTile )
        {
            foreach ( var tile in sys.Map.GetTilesWithinAbsoluteRange( currentTile.Position, unitUnderCursor.AttackRange ) )
            {
                Unit unitCheck;
                if ( sys.Map.UnitGametileMap.TryGetValue( sys.Map[ tile ], out unitCheck ) )
                    yield return unitCheck;
            }
            yield break;
        }

        public void OnRenderObject()
        {
            sys.Map.RenderForPath( TilesToPass );
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
        #endregion

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
            sys.StartCoroutine(
                General.CustomAnimation.InterpolateBetweenPoints(
                    SelectedUnit.transform,
                    TilesToPass.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(),
                    0.11f ) );

            sys.Map.SwapUnit( UnitTile, to);
        }

        public override void CursorMoved()
        {
            bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
            if ( withinMoveRange )
            {
                AttemptToLengthenPath();
            }
        }

        private void AttemptToLengthenPath()
        {
            bool tooFarFromLast = false;
            if ( TilesToPass.Count > 0 )
                tooFarFromLast = TilesToPass.First.Value.Position
                    .ManhattanDistance( sys.Cursor.CurrentTile.Position ) > 1;

            if ( TilesToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
            {
                TilesToPass = new LinkedList<GameTile>( MapSearcher.Search( UnitTile, sys.Cursor.CurrentTile, sys.Map, SelectedUnit.MovementRange ) );
                return;
            }

            var foundNode = TilesToPass.Find( sys.Cursor.CurrentTile );
            if ( foundNode != null )
            {
                while ( TilesToPass.First != foundNode )
                {
                    TilesToPass.RemoveFirst();
                }
            }
            else 
            {
                TilesToPass.AddFirst( sys.Cursor.CurrentTile );
            }
        }
    }
}