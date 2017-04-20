using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent( typeof( GameMap ) )]
public class MapGameObjectDecorator : MapDecorator
{
    private GameMap Map;
    public GameObject MovementObj;
    public GameObject SelectionObj;
    private List<GameObject> movementObjBuffer;
    private List<GameObject> pointsObjBuffer;

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
    }
}

