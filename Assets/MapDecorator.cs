using Assets.General.DataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent( typeof( GameMap ) )]
public class MapDecorator : MonoBehaviour
{
    private static MapDecorator instance;
    public static MapDecorator Instance { get { return instance; } }

    private CommandBuffer UnitMovementBuffer;

    public void Awake()
    {
        instance = this;
        UnitMovementBuffer = new CommandBuffer();
        Camera.main.AddCommandBuffer( CameraEvent.AfterGBuffer, UnitMovementBuffer );
    }

    public void Start()
    {
    }

    public void ShowUnitMovementIfHere( GameTile tile )
    {
        Unit unitThere;
        BattleSystem.Instance.Map.UnitGametileMap.TryGetValue( tile, out unitThere );
        if ( unitThere != null )
            ShowUnitMovement( unitThere );
    }

    public void ShowUnitMovement( Unit unit )
    {
        UnitMovementBuffer.Clear();
        if ( unit == null )
            return;

        List<Vector2Int> validMovementTiles =
            BattleSystem.Instance.Map.GetValidMovementPositions( unit, BattleSystem.Instance.Map.UnitGametileMap[ unit ] ).ToList();

        foreach ( var tile in validMovementTiles.Select( tile => BattleSystem.Instance.Map[ tile ] ) )
        {
            UnitMovementBuffer.DrawMesh( tile.GetComponent<MeshFilter>().mesh,
                tile.transform.localToWorldMatrix,
                BattleSystem.Instance.Map.MovementMat, 0 );
        }

        foreach ( var tile in
            BattleSystem.Instance.Map.GetFringeAttackTiles(
                new HashSet<Vector2Int>( validMovementTiles ), unit.AttackRange )
            .Select( tile => BattleSystem.Instance.Map[ tile ] ) )
        {
            UnitMovementBuffer.DrawMesh( tile.GetComponent<MeshFilter>().mesh,
                tile.transform.localToWorldMatrix,
                BattleSystem.Instance.Map.AttackRangeMat, 0 );
        }
    }
}