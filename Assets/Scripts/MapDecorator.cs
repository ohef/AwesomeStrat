using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent( typeof( GameMap ) )]
public class MapDecorator : MonoBehaviour
{
    private GameMap Map;

    public Color MovementColor;
    public Color PathColor;
    public Color CursorColor;

    public GameObject SelectionObject;

    public MapTileRenderer MovementRenderer;
    public MapTileRenderer PathRenderer;
    public MapTileRenderer CursorRenderer;

    public void Start()
    {
        Map = BattleSystem.Instance.Map;
        MovementRenderer = new MapTileRenderer( SelectionObject, MovementColor );
        CursorRenderer = new MapTileRenderer( SelectionObject, CursorColor );
        PathRenderer = new MapTileRenderer( SelectionObject, PathColor );
    }

    private class AbilityTileVisitor : IAbilityVisitor<object>
    {
        public MapDecorator decorator;
        public IEnumerable<Vector2Int> movePositions;

        public object Visit( WaitAbility ability )
        {
            return null;
        }

        public object Visit( AreaOfEffectAbility ability )
        {
            return null;
        }

        public object Visit( TargetAbility ability )
        {
            var targetTiles = decorator.Map.GetFringeAttackTiles( new HashSet<Vector2Int>( movePositions ), ability.Range );
            foreach ( var pos in targetTiles )
                decorator.MovementRenderer.ShadeAtPosition( pos, ability.TileColor );

            return null;
        }
    }

    public void ShowUnitMovement( Unit unit )
    {
        MovementRenderer.ResetBuffer();

        if ( unit == null )
            return;

        List<Vector2Int> validMovementTiles =
            Map.GetValidMovementPositions( unit ).ToList();

        foreach ( var tile in validMovementTiles )
            MovementRenderer.ShadeAtPosition( tile, MovementColor );

        var visitor = new AbilityTileVisitor { decorator = this, movePositions = validMovementTiles };
        foreach ( var ability in unit.Abilities )
        {
            ability.Accept( visitor );
        }
    }

    public void RenderMovePath( IEnumerable<Vector2Int> path )
    {
        PathRenderer.ResetBuffer();
        foreach ( var val in path )
        {
            PathRenderer.ShadeAtPosition( val );
        }
    }
}

public class MapTileRenderer
{
    private GameObject SelectionObj;
    private Color DefaultColor;
    private List<GameObject> Buffer;

    public MapTileRenderer( GameObject selectionObj, Color color )
    {
        Buffer = new List<GameObject>();
        SelectionObj = selectionObj;
        DefaultColor = color;
    }

    public void ResetBuffer()
    {
        if ( Buffer != null )
        {
            foreach ( var obj in Buffer )
                GameObject.Destroy( obj );
            Buffer.Clear();
        }
    }

    public void ShadeAtPosition( Vector2Int position, Color color, bool rePaint = false )
    {
        if ( rePaint == true ) ResetBuffer();
        Buffer.Add( InstantiateDecoratorObjectForTile( position.ToVector3(), SelectionObj, color ) );
    }

    public void ShadeAtPosition( Vector2Int position, bool rePaint = false )
    {
        ShadeAtPosition( position, DefaultColor, rePaint );
    }

    private GameObject InstantiateDecoratorObjectForTile( Vector3 position, GameObject prefab, Color color )
    {
        var instantiated = GameObject.Instantiate( prefab );
        instantiated.hideFlags = HideFlags.HideInHierarchy;
        instantiated.transform.position = position;
        instantiated.GetComponent<Renderer>().material.SetColor( "_Color", color );
        return instantiated;
    }
}