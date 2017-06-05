using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System.Linq;
using Assets.Scripts.General;

//[CustomPropertyDrawer(typeof(TileToVector2Int))]
//public class TileToVector2IntEditor : PropertyDrawer
//{
//    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
//    {
//        base.OnGUI( position, property, label );
//    }
//}

public class GameMap : MonoBehaviour, ISerializationCallbackReceiver
{
    public int Width;
    public int Height;

    [Serializable]
    public struct UnitToVector2Int
    {
        public Unit unit;
        public int x;
        public int y;
    }

    [Serializable]
    public struct TileToVector2Int
    {
        public GameTile tile;
        public int x;
        public int y;
    }

    [SerializeField]
    private List<UnitToVector2Int> SerializeUnitPos = new List<UnitToVector2Int>();
    [SerializeField]
    private List<TileToVector2Int> SerializeTilePos = new List<TileToVector2Int>();

    public void OnBeforeSerialize()
    {
        SerializeUnitPos.Clear();
        SerializeTilePos.Clear();

        foreach ( KeyValuePair<Unit,Vector2Int> kvp in UnitPos )
        {
            SerializeUnitPos.Add( new UnitToVector2Int { unit = kvp.Key, x = kvp.Value.x, y = kvp.Value.y } );
        }

        foreach ( KeyValuePair<GameTile,Vector2Int> kvp in TilePos )
        {
            SerializeTilePos.Add( new TileToVector2Int { tile = kvp.Key, x = kvp.Value.x, y = kvp.Value.y } );
        }
    }

    public void OnAfterDeserialize()
    {
        foreach ( var kvp in SerializeTilePos )
        {
            TilePos.Add( kvp.tile, new Vector2Int( kvp.x, kvp.y ) );
        }

        foreach ( var kvp in SerializeUnitPos )
        {
            UnitPos.Add( kvp.unit, new Vector2Int( kvp.x, kvp.y ) );
        }
    }

    [HideInInspector]
    public DoubleDictionary<Unit, Vector2Int> UnitPos = new DoubleDictionary<Unit, Vector2Int>();
    [HideInInspector]
    public DoubleDictionary<GameTile, Vector2Int> TilePos = new DoubleDictionary<GameTile, Vector2Int>();

    public IEnumerable<Vector2Int> AllMapPositions()
    {
        for ( int i = 0 ; i < Width ; i++ )
        {
            for ( int j = 0 ; j < Height ; j++ )
            {
                yield return new Vector2Int( i, j );
            }
        }
    }

    private IEnumerable<int> TrianglesForPosition( Vector2Int pos )
    {
        return TrianglesForPosition( pos.x, pos.y );
    }

    private IEnumerable<int> TrianglesForPosition( int i, int j )
    {
        int indiceFormat = j + i * ( Height + 1 );

        yield return indiceFormat;
        yield return indiceFormat + 1;
        yield return indiceFormat + 1 + Height + 1;

        yield return indiceFormat;
        yield return indiceFormat + 1 + Height + 1;
        yield return indiceFormat + Height + 1;
    }

    public IEnumerable<Vector2Int> GetTilesWithinAbsoluteRange( Vector2Int startingPos, int range )
    {
        IEnumerable<int> rangeInterval = CustomMath.Range( -range, range );
        IEnumerable<int> xInterval = rangeInterval.Select( i => startingPos.x + i );
        IEnumerable<int> yInterval = rangeInterval.Select( i => startingPos.y + i );
        return xInterval
            .SelectMany( x => yInterval.Select( y => new Vector2Int( x, y ) ) )
            .Where( v => IsOutOfBounds( v ) == false && ( startingPos - v ).AbsoluteNormal() <= range );
    }

    public IEnumerable<Unit> GetUnitsWithinRange( Vector2Int startingPos, int range )
    {
        foreach ( Vector2Int pos in GetTilesWithinAbsoluteRange( startingPos, range ) )
        {
            Unit unit;
            if ( UnitPos.TryGetValue( pos, out unit ) )
                yield return unit;
        }
        yield break;
    }

    public IEnumerable<Vector2Int> GetValidMovementPositions( Unit unit )
    {
        Vector2Int unitPosition = UnitPos[ unit ];
        return GetTilesWithinAbsoluteRange( unitPosition, unit.MovementRange )
            .Where( tilePos => MapSearcher.Search( unitPosition, tilePos, this, unit.MovementRange ) != null );
    }

    public HashSet<Vector2Int> GetAttackTiles( HashSet<Vector2Int> movementTiles, int attackRange )
    {
        var temp = GetFringeAttackTiles( movementTiles, attackRange );
        temp.UnionWith( movementTiles );
        return temp;
    }

    public HashSet<Vector2Int> GetFringeAttackTiles( HashSet<Vector2Int> movementTiles, int attackRange )
    {
        HashSet<Vector2Int> attackTiles = new HashSet<Vector2Int>();

        foreach ( Vector2Int tile in movementTiles )
            foreach ( Vector2Int direction in Vector2Int.AllDirections )
                foreach ( int coef in Enumerable.Range( 1, attackRange ) )
                {
                    Vector2Int neighbour = tile + direction * coef;
                    if ( IsOutOfBounds( neighbour ) == false && movementTiles.Contains( neighbour ) == false )
                        attackTiles.Add( neighbour );
                }

        return attackTiles;
    }

    private Mesh CreateGridMesh( int width, int height )
    {
        Vector2[] vertices = new Vector2[ ( width + 1 ) * ( height + 1 ) ];
        int[] triangles = new int[ width * height * 6 ];

        for ( int i = 0 ; i < width + 1 ; i++ )
            for ( int j = 0 ; j < height + 1 ; j++ )
            {
                int indiceFormat = j + i * ( height + 1 );
                vertices[ indiceFormat ] = new Vector2( i, j );
            }

        for ( int i = 0 ; i < width ; i++ )
            for ( int j = 0 ; j < height ; j++ )
            {
                int indiceFormat = j + i * ( height + 1 );
                int triIndiceFormat = ( j + ( i * height ) ) * 6;

                //Top Tri
                triangles[ triIndiceFormat ] = indiceFormat;
                triangles[ triIndiceFormat + 1 ] = indiceFormat + 1;
                triangles[ triIndiceFormat + 2 ] = indiceFormat + 1 + height + 1;


                //Lower Tri
                triangles[ triIndiceFormat + 3 ] = indiceFormat;
                triangles[ triIndiceFormat + 4 ] = indiceFormat + 1 + height + 1;
                triangles[ triIndiceFormat + 5 ] = indiceFormat + height + 1;
            }

        var mesh = new Mesh();
        mesh.vertices = vertices.Select( vert => new Vector3( vert.x, 0, vert.y ) ).ToArray<Vector3>();
        mesh.triangles = triangles;
        mesh.normals = vertices.Select( vert => Vector3.up ).ToArray();

        return mesh;
    }

    private static Mesh CreateVertexGrid( List<Vector2Int> positions )
    {
        Vector2[] vertices = new Vector2[ positions.Count * 4 ];
        int[] triangles = new int[ positions.Count * 6 ];

        {
            int i = 0;
            foreach ( Vector2Int pos in positions )
            {
                vertices[ i ] = new Vector2( pos.x, pos.y );
                vertices[ i + 1 ] = new Vector2( pos.x + 1, pos.y );
                vertices[ i + 2 ] = new Vector2( pos.x, pos.y + 1 );
                vertices[ i + 3 ] = new Vector2( pos.x + 1, pos.y + 1 );
                i += 4;
            }
        }

        {
            int i = 0;
            int j = 0;
            for ( int k = 0 ; k < positions.Count ; k++ )
            {
                //Assuming Clockwise orientation
                //Triangle 1
                triangles[ j ] = i;
                triangles[ j + 1 ] = i + 3;
                triangles[ j + 2 ] = i + 1;

                //Triangle 2
                triangles[ j + 3 ] = i;
                triangles[ j + 4 ] = i + 2;
                triangles[ j + 5 ] = i + 3;

                i += 4;
                j += 6;
            }
        }

        var mesh = new Mesh();
        mesh.vertices = vertices.Select( vert => new Vector3( vert.x, 0, vert.y ) ).ToArray<Vector3>();
        mesh.triangles = triangles;

        return mesh;
    }

    public void InitializeMap( int width, int height, GameTile prefabTile, bool clearMap = true, Vector2IntExt.Axis primaryAxis = Vector2IntExt.Axis.Y )
    {
        if ( clearMap == true ) ClearMap();

        Width = width;
        Height = height;
        GameObject TileLayer = GameObject.Find( "TileLayer" );
        foreach ( var pos in AllMapPositions() )
        {
            var gameTile = Instantiate( prefabTile, TileLayer.transform, false );
            TilePos.Add( gameTile, pos );
            gameTile.transform.localPosition = pos.ToVector3( axis: primaryAxis );
            gameTile.name = pos.ToString();
        }
    }

    public void ClearMap()
    {
        foreach ( var tile in GameObject.FindObjectsOfType<GameTile>() )
            GameObject.DestroyImmediate( tile.gameObject );

        foreach ( var unit in GameObject.FindObjectsOfType<Unit>() )
            GameObject.DestroyImmediate( unit.gameObject );

        UnitPos.Clear();
        TilePos.Clear();
    }

    public bool Occupied( Vector2Int pos )
    {
        Unit u;
        return UnitPos.TryGetValue( pos, out u );
    }

    public void PlaceUnit( Unit unit, Vector2Int pos )
    {
        if ( Occupied( pos ) == false )
            UnitPos.Add( unit, pos );
    }

    public Vector2Int ClampWithinMap( Vector2Int toClamp )
    {
        toClamp.x = CustomMath.ClampNumber( toClamp.x, 0, Width - 1 );
        toClamp.y = CustomMath.ClampNumber( toClamp.y, 0, Height - 1 );
        return toClamp;
    }

    public Vector2 ClampWithinMap( Vector2 toClamp )
    {
        toClamp.x = CustomMath.ClampNumber( toClamp.x, 0, Width - 1 );
        toClamp.y = CustomMath.ClampNumber( toClamp.y, 0, Height - 1 );
        return toClamp;
    }

    public bool IsOutOfBounds( Vector2Int v )
    {
        return CustomMath.IsOverBound( v.x, 0, Width - 1 ) || CustomMath.IsOverBound( v.y, 0, Height - 1 );
    }

    public bool IsOutOfBounds( Vector2 v )
    {
        return CustomMath.IsOverBound( v.x, 0, Width - 1 ) || CustomMath.IsOverBound( v.y, 0, Height - 1 );
    }
}