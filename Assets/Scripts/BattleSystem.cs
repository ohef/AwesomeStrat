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

        private Stack<BattleState> StateStack = new Stack<BattleState>();
        public BattleState CurrentState {
            get { return StateStack.Peek(); }
            set
            {
                if ( StateStack.Count > 0 )
                {
                    StateStack.Peek().Exit( this );
                    value.Enter( this );
                }
                StateStack.Push( value );
            }
        }

        private BattleState DefaultState = new PlayerSelectingUnit();

        public GameMap Map;
        public CursorControl Cursor;
        public CommandMenu Menu;

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
            IPlayerState poppedState = StateStack.Pop();
            poppedState.Exit( this );
            CurrentState.Enter( this );
        }

        public void GoToDefaultState()
        {
            IPlayerState poppedState = StateStack.Pop();
            poppedState.Exit( this );
            DefaultState.Enter( this );
            StateStack.Clear();
            CurrentState = DefaultState;
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
        protected BattleSystem sys { get { return BattleSystem.Instance; } }

        public virtual void CursorMoved() { }

        public virtual void Update( BattleSystem sys ) { }

        public virtual void Enter( BattleSystem sys ) { }

        public virtual void Exit( BattleSystem sys ) { }
    }

    public class NullState : BattleState { }

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

                    Animator unitAnimator = unitAtTile.GetComponentInChildren<Animator>();
                    unitAnimator.SetBool( "Selected", true );
                    sys.CurrentState = new BeforeMoveSelect( unitAtTile );
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

    public class MenuState : BattleState
    {
        public override void Enter( BattleSystem sys ) { }

        public override void Exit( BattleSystem sys )
        {
            //To prevent the next state from catching the submit button
            Input.ResetInputAxes();
            sys.Menu.ClearButtons();
        }

        public override void Update( BattleSystem sys )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
            {
                sys.GoToPreviousState();
            }
        }
    }

    public class BeforeMoveSelect : MenuState
    {
        private Unit SelectedUnit;

        public BeforeMoveSelect( Unit selectedUnit )
        {
            SelectedUnit = selectedUnit;
        }

        public override void Enter( BattleSystem sys )
        {
            //TODO: Put this in a member var
            Button addedButton = sys.Menu.AddButton( "Move", StartMoving );
            EventSystem.current.SetSelectedGameObject( addedButton.gameObject );
        }

        private void StartMoving()
        {
            sys.CurrentState = new PlayerUnitAction( SelectedUnit, sys.Map.UnitGametileMap[ SelectedUnit ] );
            SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Selected", false );
        }
    }

    public class AfterMoveSelect : MenuState
    {
        private Unit SelectedUnit;

        //public AfterMoveSelect(Unit selectedUnit, )

        public override void Enter( BattleSystem sys )
        {
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
            MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, UnitTile ) );
            AttackTiles = sys.Map.GetAttackTiles( MovementTiles, SelectedUnit.AttackRange );
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
                sys.GoToDefaultState();
            }
        }

        private IEnumerable<Unit> GetAttackableUnits( Unit unitUnderCursor, GameTile currentTile )
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
        }

        public override void Exit( BattleSystem sys )
        {
            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        }
        #endregion

        private void ExecuteAttack( Unit selectedUnit, Unit unitUnderCursor )
        {
            GameTile lastTile = TilesToPass.First();
            if ( lastTile != UnitTile ) //We need to move
            {
                ExecuteMove( lastTile );
            }
            unitUnderCursor.HP -= selectedUnit.Attack - unitUnderCursor.Defense;
        }

        //private void ExecuteAttack( Unit selectedUnit, Unit unitUnderCursor )
        //{
        //    GameTile optimalAttackPos = GetOptimalAttackPosition( sys.Cursor.CurrentTile );
        //    if ( optimalAttackPos != UnitTile ) //We need to move
        //    {
        //        ExecuteMove( optimalAttackPos );
        //    }
        //    unitUnderCursor.HP -= selectedUnit.Attack - unitUnderCursor.Defense;
        //}

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
            SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", true );
            sys.StartCoroutine(
                General.CustomAnimation.InterpolateBetweenPointsAndCallback(
                    SelectedUnit.transform,
                    TilesToPass.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(),
                    0.22f, () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false ) ) );

            sys.Map.SwapUnit( UnitTile, to);
        }

        public override void CursorMoved()
        {
            bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
            if ( withinMoveRange )
            {
                AttemptToLengthenPath( sys.Cursor.CurrentTile );
            }
        }

        private void AttemptToLengthenPath( GameTile to )
        {
            bool tooFarFromLast = false;
            if ( TilesToPass.Count > 0 )
                tooFarFromLast = TilesToPass.First.Value.Position
                    .ManhattanDistance( to.Position ) > 1;

            if ( TilesToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
            {
                TilesToPass = new LinkedList<GameTile>( MapSearcher.Search( UnitTile, to, sys.Map, SelectedUnit.MovementRange ) );
                return;
            }

            var foundNode = TilesToPass.Find( to );
            if ( foundNode != null )
            {
                while ( TilesToPass.First != foundNode )
                {
                    TilesToPass.RemoveFirst();
                }
            }
            else 
            {
                TilesToPass.AddFirst( to );
            }
        }
    }
}