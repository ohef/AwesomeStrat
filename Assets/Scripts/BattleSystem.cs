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
        private CursorControl mapCursor;
        private GameMap map;

        public PlayerSelectingUnit( GameMap map )
        {
            mapCursor = map.GetComponentInChildren<CursorControl>();
            this.map = map;
        }

        public IPlayerState Update( IPlayerState currentState )
        {
            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if ( direction.x != 0 || direction.y != 0 )
                mapCursor.MoveCursor( direction );

            var tile = mapCursor.CurrentTile;
            if ( tile != null && tile.UnitOccupying != null && Input.GetButtonDown( "Jump" ) )
            {
                map.RenderSelection( new Vector2Int[] { mapCursor.CurrentTile.Position } );
                return new PlayerSelectingForAttacks( map, tile.UnitOccupying );
            }

            return currentState;
        }
    }

    protected class PlayerSelectingForAttacks : IPlayerState
    {
        private Unit _SelectedUnit;
        private GameMap _Map;

        public PlayerSelectingForAttacks( GameMap map, Unit selectedUnit )
        {
            _Map = map;
            _SelectedUnit = selectedUnit;
        }

        IPlayerState IPlayerState.Update( IPlayerState state )
        {
            return state;
        }
    }
    #endregion

    protected IPlayerState _State;
    private static PlayerSelectingUnit playerSelectingUnit = null;
    private static PlayerSelectingForAttacks playerSelectingForAttacks = null;
    public PlayerState( GameMap map )
    {
        _State = playerSelectingUnit = new PlayerSelectingUnit( map );
    }

    public void Update()
    {
        _State = _State.Update( _State );
    }
}
