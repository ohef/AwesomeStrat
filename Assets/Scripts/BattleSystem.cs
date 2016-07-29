using UnityEngine;
using System.Collections;
using System;
using Assets.General.DataStructures;
using Assets.Map;

public class BattleSystem : MonoBehaviour
{
    private enum BattleTurn
    {
        Player,
        Enemy
    };

    public GameMap map;
    private PlayerState _PlayerState;
    private EnemyState _EnemyState;
    private BattleTurn _Turn;

    void Awake()
    {
        _PlayerState = new PlayerState( map );
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        switch(_Turn)
        {
            case BattleTurn.Player:
                _PlayerState.Update();
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

public class PlayerState
{

    #region Interfaces
    protected interface IPlayerState
    {
        IPlayerState Update( IPlayerState state );
    }
    #endregion

    #region Classes
    protected class PlayerSelectingUnit : IPlayerState
    {
        private CursorControl _MapCursor;
        private GameMap _Map;

        public PlayerSelectingUnit( GameMap map, CursorControl cursor )
        {
            _MapCursor = cursor;
            this._Map = map;
        }

        public IPlayerState Update( IPlayerState currentState )
        {
            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if ( direction.x != 0 || direction.y != 0 )
                _MapCursor.MoveCursor( direction );

            var tile = _MapCursor.CurrentTile;
            if ( tile != null )
            {
                if ( tile.UnitOccupying != null )
                {
                    _Map.RenderUnitMovement( tile.UnitOccupying, 0.5f );
                    if ( Input.GetButtonDown( "Jump" ) )
                    {
                        return new PlayerSelectingForAttacks( _Map, _MapCursor, tile.UnitOccupying );
                    }
                }
                else
                    _Map.StopRenderingOverlays();
            }

            return currentState;
        }
    }

    protected class PlayerSelectingForAttacks : IPlayerState
    {
        private Unit _SelectedUnit;
        private GameMap _Map;
        private CursorControl _MapCursor;

        public PlayerSelectingForAttacks( GameMap map, CursorControl cursor, Unit selectedUnit )
        {
            _Map = map;
            _SelectedUnit = selectedUnit;
            _MapCursor = cursor;
        }

        IPlayerState IPlayerState.Update( IPlayerState state )
        {
            _Map.RenderUnitMovement( _SelectedUnit );

            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if ( direction.x != 0 || direction.y != 0 )
                _MapCursor.MoveCursor( direction );

            if ( Input.GetButtonDown( "Jump" ) )
            {
                return playerSelectingUnit;
            }

            return state;
        }
    }
    #endregion

    protected IPlayerState _State;
    private static PlayerSelectingUnit playerSelectingUnit = null;
    private static PlayerSelectingForAttacks playerSelectingForAttacks = null;
    public PlayerState( GameMap map )
    {
        _State = playerSelectingUnit = new PlayerSelectingUnit( map, map.GetComponentInChildren<CursorControl>() );
    }

    public void Update()
    {
        _State = _State.Update( _State );
    }
}
