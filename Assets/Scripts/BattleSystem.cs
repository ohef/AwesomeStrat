using UnityEngine;
using System.Collections;
using System;
using Assets.General.DataStructures;
using Assets.Map;

public class BattleSystem : MonoBehaviour {
    private enum BattleTurn
    {
        Player,
        Enemy
    };

    protected interface IPlayerState
    {
        void Update(IPlayerState state);
    }

    protected IPlayerState turnState;

    protected class PlayerSelectingUnit : IPlayerState
    {
        private CursorControl mapCursor;
        private GameMap map;

        public PlayerSelectingUnit( GameMap map )
        {
            mapCursor = map.GetComponentInChildren<CursorControl>();
            this.map = map;
        }

        public void Update( IPlayerState currentState )
        {
            var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );
            if( direction.x != 0 || direction.y != 0 )
                mapCursor.MoveCursor( direction );

            var tile = mapCursor.CurrentTile;
            if ( tile != null && tile.UnitOccupying != null && Input.GetButton( "Submit" ) )
            {
                currentState = new PlayerSelectingForAttacks( map, tile.UnitOccupying );
            }
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

        public void Update( IPlayerState state )
        {
        }
    }

    private static PlayerSelectingUnit playerSelectingUnit;
    private static PlayerSelectingForAttacks playerSelectingForAttacks;
    public GameMap map;

    void Awake()
    {
        playerSelectingUnit = new PlayerSelectingUnit( map );
        turnState = playerSelectingUnit;
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        turnState.Update(turnState);
	}
}
