using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Assets.General.DataStructures;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Assets.General.UnityExtensions;

namespace Assets.Map
{
    public class BattleSystem : MonoBehaviour
    {
        private static BattleSystem instance;
        public static BattleSystem Instance { get { return instance; } }

        private Stack<BattleState> TurnStateStack = new Stack<BattleState>();
        public BattleState TurnState { get { return CurrentTurn.State; } set { CurrentTurn.State = value; } }

        public TurnState CurrentTurn;

        public GameMap Map;
        public CursorControl Cursor;
        public CommandMenu Menu;
        public UnityEvent StateChanged;
        public event Action InRenderObject;

        void Awake()
        {
            instance = this;
        }

        // Use this for initialization
        void Start()
        {
            CurrentTurn = new PlayerTurn();
        }

        // Update is called once per frame
        void Update()
        {
            CurrentTurn.Update( this );
        }

        private void OnRenderObject()
        {
            if ( InRenderObject != null )
                InRenderObject();
        }
    }

    public abstract class TurnState : IPlayerState
    {
        protected BattleSystem sys { get { return BattleSystem.Instance; } }

        private Stack<BattleState> StateStack = new Stack<BattleState>();
        public BattleState State {
            get { return StateStack.Peek(); }
            set
            {
                if ( StateStack.Count > 0 )
                {
                    StateStack.Peek().Exit( sys );
                    value.Enter( sys );
                }
                StateStack.Push( value );
            }
        }

        public void GoToPreviousState()
        {
            if ( State == ChoosingUnitForAction.Instance )
                return;

            IPlayerState poppedState = StateStack.Pop();
            poppedState.Exit( sys );
            State.Enter( sys );
        }

        public void GoToStateAndForget( BattleState state )
        {
            IPlayerState poppedState = StateStack.Pop();
            poppedState.Exit( sys );
            state.Enter( sys );
            StateStack.Clear();
            State = state;
        }

        public virtual void Enter( BattleSystem sys ) { }

        public virtual void Exit( BattleSystem sys ) { }

        public virtual void Update( BattleSystem sys )
        {
            State.Update( sys );
        }
    }

    public interface IPlayerState
    {
        void Update( BattleSystem state );
        void Enter( BattleSystem state );
        void Exit( BattleSystem state );
    }

    public class PlayerTurn : TurnState
    {
        public PlayerTurn()
        {
            State = ChoosingUnitForAction.Instance;
            State.Enter( sys );
        }
    }

    public abstract class BattleState : IPlayerState
    {
        protected BattleSystem sys { get { return BattleSystem.Instance; } }

        public virtual void Update( BattleSystem sys ) { }

        public virtual void Enter( BattleSystem sys ) { }

        public virtual void Exit( BattleSystem sys ) { }
    }

    public abstract class ControlCursor : BattleState
    {
        public override void Update( BattleSystem sys )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
                sys.CurrentTurn.GoToPreviousState();

            sys.Cursor.UpdateAction();
        }
    }

    public abstract class MenuState : BattleState
    {
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
                sys.CurrentTurn.GoToPreviousState();
            }
        }
    }

    public class ChoosingUnitForAction : ControlCursor
    {
        private static ChoosingUnitForAction instance = new ChoosingUnitForAction();
        public static ChoosingUnitForAction Instance
        {
            get
            {
                return instance;
            }
        }

        public override void Update( BattleSystem sys )
        {
            base.Update( sys );
            MapUnit unitAtTile;
            if ( sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitAtTile ) )
            {
                if ( Input.GetButtonDown( "Submit" ) && unitAtTile.hasTakenAction == false )
                {
                    Animator unitAnimator = unitAtTile.GetComponentInChildren<Animator>();
                    unitAnimator.SetBool( "Selected", true );
                    sys.TurnState = new SelectUnitActions( unitAtTile );
                }
            }
        }

        public override void Enter( BattleSystem sys )
        {
            sys.Cursor.CursorMoved.AddListener( CursorMoved );
        }

        public override void Exit( BattleSystem sys )
        {
            sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
        }

        public void CursorMoved()
        {
            sys.Map.ShowUnitMovementIfHere( sys.Cursor.CurrentTile );
        }
    }

    public class SelectUnitActions : MenuState
    {
        private MapUnit SelectedUnit;
        private GameTile SelectedTile { get { return sys.Map.UnitGametileMap[ SelectedUnit ]; } }

        public SelectUnitActions( MapUnit selectedUnit )
        {
            SelectedUnit = selectedUnit;
        }

        public override void Enter( BattleSystem sys )
        {
            Button defaultSelected = sys.Menu.AddButton( "Wait", Wait );

            if ( SelectedUnit.hasMoved == false )
                sys.Menu.AddButton( "Move", StartMoving );

            var interactables = GetAttackableUnits();
            if ( interactables.Count() > 0 )
                defaultSelected = sys.Menu.AddButton( "Attack", () => StartChooseAttacks( interactables ) );

            EventSystem.current.SetSelectedGameObject( defaultSelected.gameObject );
        }

        private void StartMoving()
        {
            sys.TurnState = new WhereToMove( SelectedUnit );
            SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Selected", false );
        }

        private IEnumerable<MapUnit> GetAttackableUnits()
        {
            foreach ( var tile in sys.Map.GetTilesWithinAbsoluteRange( SelectedTile.Position, SelectedUnit.AttackRange ) )
            {
                if ( tile == SelectedTile.Position )
                    continue;

                MapUnit unitCheck = null;
                if ( sys.Map.UnitGametileMap.TryGetValue( sys.Map[ tile ], out unitCheck ) )
                    yield return unitCheck;
            }
            yield break;
        }

        private void Wait()
        {
            sys.CurrentTurn.GoToStateAndForget( ChoosingUnitForAction.Instance );
            SelectedUnit.hasTakenAction = true;
        }

        private void StartChooseAttacks( IEnumerable<MapUnit> interactables )
        {
            sys.TurnState = new ChooseAttacks( SelectedUnit, interactables.Where( i => i is MapUnit ).Cast<MapUnit>() );
        }
    }

    public class ChooseAttacks : BattleState
    {
        private MapUnit SelectedUnit;
        private LinkedList<MapUnit> ToAttack;
        private LinkedListNode<MapUnit> CurrentlySelected;

        public ChooseAttacks( MapUnit selectedUnit, IEnumerable<MapUnit> toAttack )
        {
            SelectedUnit = selectedUnit;
            ToAttack = new LinkedList<MapUnit>( toAttack );
            CurrentlySelected = ToAttack.First;
            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
        }

        public override void Update( BattleSystem sys )
        {
            if ( Input.GetButtonDown( "Cancel" ) )
            {
                sys.CurrentTurn.GoToPreviousState();
            }

            HandleChoosing( sys );

            if ( Input.GetButtonDown( "Submit" ) )
            {
                SelectedUnit.AttackUnit( CurrentlySelected.Value );
                sys.CurrentTurn.GoToStateAndForget( ChoosingUnitForAction.Instance );
                SelectedUnit.hasTakenAction = true;
            }
        }

        public override void Exit( BattleSystem sys )
        {
            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        }

        public override void Enter( BattleSystem sys )
        {
            sys.Map.ShowStandingAttackRange( SelectedUnit );
        }

        private void HandleChoosing( BattleSystem sys )
        {
            Vector2Int input = Vector2IntExt.GetInputAsDiscrete();
            if ( ToAttack.Count > 0 )
            {
                if ( input.x == 1 )
                {
                    if ( CurrentlySelected.Next == null )
                        CurrentlySelected = ToAttack.First;
                    else
                        CurrentlySelected = CurrentlySelected.Next;

                    sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
                }
                else if ( input.x == -1 )
                {
                    if ( CurrentlySelected.Previous == null )
                        CurrentlySelected = ToAttack.Last;
                    else
                        CurrentlySelected = CurrentlySelected.Previous;

                    sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ CurrentlySelected.Value ].Position );
                }
            }
        }
    }

    public class WhereToMove : ControlCursor
    {
        private MapUnit SelectedUnit;
        private GameTile SelectedTile { get { return sys.Map.UnitGametileMap[ SelectedUnit ]; } }
        private HashSet<Vector2Int> MovementTiles;
        private LinkedList<GameTile> TilesToPass ;

        public WhereToMove( MapUnit selectedUnit )
        {
            SelectedUnit = selectedUnit;
        }

        public override void Update( BattleSystem sys )
        {
            base.Update( sys );
            bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );

            MapUnit unitUnderCursor = null;
            sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitUnderCursor );

            if ( Input.GetButtonDown( "Submit" ) )
            {
                bool notTheSameUnit = unitUnderCursor != SelectedUnit;
                if ( canMoveHere && notTheSameUnit )
                {
                    ExecuteMove( sys.Cursor.CurrentTile );
                    sys.TurnState = new SelectUnitActions( SelectedUnit );
                }
            }
        }

        public override void Enter( BattleSystem sys )
        {
            sys.InRenderObject += OnRenderObject;
            sys.Cursor.CursorMoved.AddListener( CursorMoved );
            TilesToPass = new LinkedList<GameTile>();
            TilesToPass.AddFirst( SelectedTile );
            MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, SelectedTile ) );
        }

        public override void Exit( BattleSystem sys )
        {
            sys.InRenderObject -= OnRenderObject;
            sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
            sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        }

        public void CursorMoved()
        {
            bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
            if ( withinMoveRange )
            {
                AttemptToLengthenPath( sys.Cursor.CurrentTile );
            }
        }

        public void OnRenderObject()
        {
            sys.Map.RenderForPath( TilesToPass );
        }

        private void ExecuteAttack( MapUnit selectedUnit, MapUnit unitUnderCursor )
        {
            GameTile lastTile = TilesToPass.First();
            if ( lastTile != SelectedTile ) //We need to move
            {
                ExecuteMove( lastTile );
            }
            selectedUnit.AttackUnit( unitUnderCursor );
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
            else if ( canMovePositions.Any( pos => pos == SelectedTile.Position ) )
                return SelectedTile;

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
                General.UnityExtensions.CoroutineHelper.AddAfter(
                    General.CustomAnimation.InterpolateBetweenPoints( SelectedUnit.transform,
                    TilesToPass.Select( x => x.GetComponent<Transform>().localPosition )
                    .Reverse().ToList(), 0.22f ),
                    () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false ) ) );

            sys.Map.SwapUnit( SelectedTile, to);
            SelectedUnit.hasMoved = true;
        }

        private void AttemptToLengthenPath( GameTile to )
        {
            bool tooFarFromLast = false;
            if ( TilesToPass.Count > 0 )
                tooFarFromLast = TilesToPass.First.Value.Position
                    .ManhattanDistance( to.Position ) > 1;

            if ( TilesToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
            {
                TilesToPass = new LinkedList<GameTile>( MapSearcher.Search( SelectedTile, to, sys.Map, SelectedUnit.MovementRange ) );
                return;
            }

            LinkedListNode<GameTile> alreadyPresent = TilesToPass.Find( to );
            if ( alreadyPresent == null )
            {
                TilesToPass.AddFirst( to );
            }
            else
            {
                while ( TilesToPass.First != alreadyPresent )
                {
                    TilesToPass.RemoveFirst();
                }
            }
        }
    }
}