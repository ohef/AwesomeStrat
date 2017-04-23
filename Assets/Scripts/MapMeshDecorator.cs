using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent( typeof( GameMap ) )]
public class MapMeshDecorator : MapDecorator
{
    private CommandBuffer UnitMovementBuffer;
    private CommandBuffer PointsBuffer;
    private GameMap Map;
    public GameObject DecoratorShader;

    public void Awake()
    {
        Map = GetComponent<GameMap>();

        UnitMovementBuffer = new CommandBuffer();
        UnitMovementBuffer.name = "Unit Movement Path";

        PointsBuffer = new CommandBuffer();
        PointsBuffer.name = "Points";

        Camera.main.AddCommandBuffer( CameraEvent.BeforeForwardAlpha, UnitMovementBuffer );
        Camera.main.AddCommandBuffer( CameraEvent.BeforeForwardAlpha, PointsBuffer );
    }

    public override void RenderForPath( IEnumerable<Vector2Int> path )
    {
        PointsBuffer.Clear();
        foreach ( var pos in path )
        {
            GameTile foundTile = Map.TilePos[ pos ];
            PointsBuffer.DrawMesh( DecoratorShader.GetComponent<MeshFilter>().mesh,
                foundTile.transform.localToWorldMatrix,
                Map.SelectionMat );
        }
    }

    public override void ShowUnitMovement( Unit unit )
    {
        UnitMovementBuffer.Clear();

        if ( unit == null )
            return;

        List<Vector2Int> validMovementTiles =
            Map.GetValidMovementPositions( unit ).ToList();

        foreach ( var tile in validMovementTiles.Select( pos => Map.TilePos[ pos ] ) )
        {
            UnitMovementBuffer.DrawMesh( 
                DecoratorShader.GetComponent<MeshFilter>().mesh,
                tile.transform.localToWorldMatrix,
                Map.MovementMat, 0 );
        }
    }
}
