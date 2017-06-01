using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent( typeof( GameMap ) )]
public class MapGameObjectDecorator : MapDecorator
{
    protected GameMap Map;
    public GameObject MovementObj;
    public GameObject SelectionObj;
    protected List<GameObject> movementObjBuffer;
    protected List<GameObject> pointsObjBuffer;

    public void Awake()
    {
        Map = GetComponent<GameMap>();
    }

    public override void RenderForPath( IEnumerable<Vector2Int> path )
    {
        ResetBuffer( pointsObjBuffer );
        pointsObjBuffer = path
            .Select( pos => Map.TilePos[ pos ] )
            .Select( tile => InstantiateDecoratorObjectForTile( tile, SelectionObj ) )
            .ToList();
    }

    private void ResetBuffer( List<GameObject> buffer )
    {
        if ( buffer != null )
        {
            foreach ( var obj in buffer )
                GameObject.Destroy( obj );
            buffer.Clear();
        }
    }

    private GameObject InstantiateDecoratorObjectForTile( GameTile tile, GameObject obj, Color color )
    {
        var instantiated = Instantiate( obj );
        instantiated.hideFlags = HideFlags.HideInHierarchy;
        instantiated.transform.position = tile.transform.position;
        instantiated.GetComponent<Renderer>().material.SetColor( "_Color", color );
        return instantiated;
    }

    private GameObject InstantiateDecoratorObjectForTile( GameTile tile, GameObject obj )
    {
        var instantiated = Instantiate( obj );
        instantiated.hideFlags = HideFlags.HideInHierarchy;
        instantiated.transform.position = tile.transform.position;
        return instantiated;
    }

    public override void ShowUnitMovement( Unit unit )
    {
        ResetBuffer( movementObjBuffer );

        if ( unit == null )
            return;

        List<Vector2Int> validMovementTiles =
            Map.GetValidMovementPositions( unit ).ToList();

        movementObjBuffer =
            validMovementTiles
            .Select( pos => Map.TilePos[ pos ] )
            .Select( tile => InstantiateDecoratorObjectForTile( tile, MovementObj ) )
            .ToList();

        var visitor = new AbilityTileVisitor { decorator = this, movePositions = validMovementTiles };
        foreach ( var ability in unit.Abilities )
        {
            movementObjBuffer.AddRange( ability.Accept( visitor ) );
        }
    }

    public class AbilityTileVisitor : IAbilityVisitor<IEnumerable<GameObject>>
    {
        public MapGameObjectDecorator decorator;
        public IEnumerable<Vector2Int> movePositions;

        public IEnumerable<GameObject> Visit( WaitAbility ability )
        {
            return Enumerable.Empty<GameObject>();
        }

        public IEnumerable<GameObject> Visit( AreaOfEffectAbility ability )
        {
            return Enumerable.Empty<GameObject>();
        }

        public IEnumerable<GameObject> Visit( TargetAbility ability )
        {
            var targetTiles = decorator.Map.GetFringeAttackTiles( new HashSet<Vector2Int>( movePositions ), ability.Range );
            return targetTiles
            .Select( pos => decorator.Map.TilePos[ pos ] )
            .Select( tile => decorator.InstantiateDecoratorObjectForTile( tile, decorator.MovementObj, ability.TileColor ) )
            .ToList();
        }
    }
}