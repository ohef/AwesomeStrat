using Assets.General.DataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent( typeof( GameMap ) )]
public class MapDecorator : MonoBehaviour
{
    private CommandBuffer UnitMovementBuffer;
    private CommandBuffer PointsBuffer;
    private GameMap Map;

    public void Awake()
    {
        UnitMovementBuffer = new CommandBuffer();
        UnitMovementBuffer.name = "Unit Movement Path";

        PointsBuffer = new CommandBuffer();
        PointsBuffer.name = "Points";

        Map = GetComponent<GameMap>();
        Camera.main.AddCommandBuffer( CameraEvent.BeforeImageEffectsOpaque, UnitMovementBuffer );
        Camera.main.AddCommandBuffer( CameraEvent.BeforeImageEffectsOpaque, PointsBuffer );
    }

    public void Start() { }

    public void RenderForPath( IEnumerable<Vector2Int> path )
    {
        PointsBuffer.Clear();
        foreach ( var pos in path )
        {
            GameTile foundTile = Map.TilePos[ pos ];
            PointsBuffer.DrawMesh( foundTile.GetComponent<MeshFilter>().mesh,
                foundTile.transform.localToWorldMatrix,
                Map.SelectionMat, 0 );
        }
    }

    public void ShowUnitMovementIfHere( Vector2Int pos )
    {
        Unit unitThere;
        Map.UnitPos.TryGetValue( pos, out unitThere );
        if ( unitThere != null )
            ShowUnitMovement( unitThere );
    }

    public void ShowUnitMovement( Unit unit )
    {
        UnitMovementBuffer.Clear();
        if ( unit == null )
            return;

        List<Vector2Int> validMovementTiles =
            Map.GetValidMovementPositions( unit ).ToList();

        foreach ( var tile in validMovementTiles.Select( pos => Map.TilePos[ pos ] ) )
        {
            UnitMovementBuffer.DrawMesh( tile.GetComponent<MeshFilter>().mesh,
                tile.transform.localToWorldMatrix,
                Map.MovementMat, 0 );
        }
    }
}