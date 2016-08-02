using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Map;
using System.Linq;
using System.IO;
using YamlDotNet.Serialization;
using System.Text;
using System.Collections;

[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
public class GameMap : MonoBehaviour
{
    public int Width;
    public int Height;

    private Mesh m_MapMesh;
    public GameTile TilePrefab;

    public Material NormalMat;
    public Material MovementMat;
    public Material AttackRangeMat;
    public Material SelectionMat;

    #region Monobehaviour Functions

    void Awake()
    {
        //TODO Instantialize Map
        InitializeMap();

        this.GetComponent<MeshFilter>().mesh = m_MapMesh = CreateGridMesh( Width, Height);
        m_MapMesh.subMeshCount = 4;

        this.GetComponent<MeshRenderer>().materials = new Material[]
            {
                NormalMat,
                MovementMat,
                AttackRangeMat,
                SelectionMat,
            };

        var collider = this.GetComponent<BoxCollider>();
        collider.size = new Vector3( Width, Height );
        collider.center = collider.size * 0.5f;

        IEnumerable<Unit> units = new Unit[] { new Unit { Attack = 1, AttackRange = 1, Defense = 2, HP = 20, Movement = 5, Position = new Vector2Int( 0, 0 ) } };
        foreach ( var unit in units )
        {
            this[ unit.Position ].UnitOccupying = unit;
        }
    }

    public void OnDrawGizmos()
    {
        if ( m_TileMap != null )
        {
            foreach ( GameTile tile in this.m_TileMap )
            {
                if ( tile.UnitOccupying != null )
                    Gizmos.DrawCube( tile.UnitOccupying.Position.ToVector3( Vector2IntExtensions.Axis.Y ) + new Vector3( 0.5f, 0.0f, 0.5f ), Vector3.one * 0.5f );
            }
        }
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    #endregion

    #region Member Functions

    private void AddTrianglesForPosition( int i, int j, List<int> triangleList )
    {
        int height = Height;
        int indiceFormat = j + i * ( height + 1 );

        //Top Tri
        triangleList.Add( indiceFormat );
        triangleList.Add( indiceFormat + 1 );
        triangleList.Add( indiceFormat + 1 + height + 1 );

        //Bottom Tri
        triangleList.Add( indiceFormat );
        triangleList.Add( indiceFormat + 1 + height + 1 );
        triangleList.Add( indiceFormat + height + 1 );
    }

    private int[] TrianglesForPosition( int i, int j )
    {
        int height = Height;
        int indiceFormat = j + i * ( height + 1 );

        return new int[] {
        indiceFormat,
        indiceFormat + 1 ,
        indiceFormat + 1 + height + 1 ,

        indiceFormat ,
        indiceFormat + 1 + height + 1 ,
        indiceFormat + height + 1 ,
        };
    }

    public IEnumerable<Vector2Int> GetTilesWithinAbsoluteRange( Vector2Int startingPos, int range)
    {
        Func<int, int> clampX = i => ClampX( i );
        Func<int, int> clampY = i => ClampY( i );

        for ( int i = clampX( startingPos.x - range ) ; i <= clampX( startingPos.x + range ) ; i++ )
            for ( int j = clampY( startingPos.y - range ) ; j <= clampY( startingPos.y + range ) ; j++ )
            {
                Vector2Int toReturn = new Vector2Int( i, j );
                if ( ( startingPos - toReturn ).AbsoluteNormal() <= range )
                    yield return toReturn;
            }
    }

    // Function that renders where a unit can move
    public void RenderUnitMovement( Unit unit, float alpha = 1.0f )
    {
        List<int> MovementSet = new List<int>();
        List<int> AttackSet = new List<int>();

        {
            HashSet<Vector2Int> movementTiles = new HashSet<Vector2Int>();

            foreach ( var position in GetTilesWithinAbsoluteRange( unit.Position, unit.Movement ) ) 
            {
                List<GameTile> result = MapSearcher.Search( this[ unit.Position ], this[ position ], this.m_TileMap, unit.Movement );
                if ( result != null )
                {
                    AddTrianglesForPosition( position.x, position.y, MovementSet );
                    movementTiles.Add( position );
                }
            }

            foreach ( var attackTile in GetAttackTiles( movementTiles, unit.AttackRange ) )
            {
                AddTrianglesForPosition( attackTile.x, attackTile.y, AttackSet );
            }
        }

        m_MapMesh.SetTriangles( MovementSet, 1 );
        m_MapMesh.SetTriangles( AttackSet, 2 );

        Func<Material, Color> switchColor = mat =>
        {
            var temp = mat.GetColor( "_Color" );
            temp.a = alpha;
            return temp;
        };

        AttackRangeMat.SetColor( "_Color", switchColor( AttackRangeMat ) );
        MovementMat.SetColor( "_Color", switchColor( MovementMat ) );
    }

    public void RenderSelection( IEnumerable<Vector2Int> tiles )
    {
        //List<int> triangles = new List<int>() ;
        //foreach ( var tile in tiles )
        //{
        //    AddTrianglesForPosition( tile.x, tile.y, triangles );
        //}
        //m_MapMesh.SetTriangles( triangles, 3 );
        m_MapMesh.SetTriangles( tiles.SelectMany( tile => TrianglesForPosition( tile.x, tile.y ) ).ToList(), 3 );
    }

    public void StopRenderingOverlays()
    {
        m_MapMesh.SetTriangles( new int[] { }, 1 );
        m_MapMesh.SetTriangles( new int[] { }, 2 );
        m_MapMesh.SetTriangles( new int[] { }, 3 );
    }

    private List<Vector2Int> GetAttackTiles( HashSet<Vector2Int> movementTiles, int attackRange )
    {
        HashSet<Vector2Int> attackTiles = new HashSet<Vector2Int>();
        foreach ( Vector2Int tile in movementTiles )
        {
            foreach ( Vector2Int direction in new Vector2Int[] { Vector2Int.Up, Vector2Int.Down, Vector2Int.Left, Vector2Int.Right } )
            {
                for ( int i = 0 ; i < attackRange ; i++ )
                {
                    Vector2Int neighbour = ClampWithinMap( tile + direction * attackRange );
                    if ( movementTiles.Contains( neighbour ) == false )
                        attackTiles.Add( neighbour );
                }
            }
        }
        return attackTiles.ToList();
    }

    private List<Vector2Int> GetAttackTilesSetImpl( HashSet<Vector2Int> movementTiles, int attackRange)
    {
        HashSet<Vector2Int> attackTiles = new HashSet<Vector2Int>();
        foreach ( var position in movementTiles )
        {
            var attackFromPos = GetTilesWithinAbsoluteRange( position, attackRange );
            attackTiles.UnionWith( attackFromPos );
        }
        attackTiles.ExceptWith( movementTiles );
        return attackTiles.ToList();
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

    private Mesh CreateVertexGrid( List<Vector2Int> positions )
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
            foreach ( Vector2Int pos in positions )
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
    #endregion

    #region ImportedMapFunctions
    public GameTile[,] m_TileMap;

    public void InitializeMap()
    {
        m_TileMap = new GameTile[ Width, Height ];
        for ( int i = 0 ; i < Width ; i++ )
            for ( int j = 0 ; j < Height ; j++ )
            {
                var gameTile = Instantiate( TilePrefab );
                gameTile.Position.x = i;
                gameTile.Position.y = j;
                this[ gameTile.Position ] = gameTile;
                gameTile.transform.SetParent( this.transform );
                gameTile.SnapToPosition();
                gameTile.name = gameTile.Position.ToString();
            }
    }

    public GameTile this[ int x, int y ]
    {
        get { return m_TileMap[ x, y ]; }
        set { m_TileMap[ x, y ] = value; }
    }

    //TODO: Error Handle plz
    public GameTile this[ Vector2Int v ]
    {
        get { return m_TileMap[ v.x, v.y ]; }
        set { m_TileMap[ v.x, v.y ] = value; }
    }

    private bool CanMoveInto( GameTile tile )
    {
        return tile.UnitOccupying == null;
    }

    private void SwapUnit( GameTile a, GameTile b )
    {
        if ( CanMoveInto( b ) == true )
        {
            b.UnitOccupying = a.UnitOccupying;
            a.UnitOccupying = null;
        }
    }

    private static bool IsOverBounded( int i, int lowerBound, int upperBound )
    {
        return i < lowerBound || i > upperBound;
    }

    private static T ClampNumber<T>( T i, T lowerBound, T upperBound ) where T : IComparable<T>
    {
        i = i.CompareTo( lowerBound ) < 0 ? lowerBound : i;
        i = i.CompareTo( upperBound ) > 0 ? upperBound : i;
        return i;
    }

    public int ClampX( int i )
    {
        return ClampNumber( i, 0, Width - 1 );
    }

    public int ClampY( int i )
    {
        return ClampNumber( i, 0, Height - 1 );
    }

    public Vector2Int ClampWithinMap( Vector2Int toClamp )
    {
        toClamp.x = ClampNumber( toClamp.x, 0, Width - 1 );
        toClamp.y = ClampNumber( toClamp.y, 0, Height - 1 );
        return toClamp;
    }

    public Vector3 ClampWithinMapViaXZPlane( Vector3 toClamp )
    {
        toClamp.x = ClampNumber( toClamp.x, 0, Width - 1 );
        toClamp.z = ClampNumber( toClamp.z, 0, Height - 1 );
        return toClamp;
    }

    public bool OutOfBounds( Vector2Int v )
    {
        return IsOverBounded( v.x, 0, Width - 1 ) || IsOverBounded( v.y, 0, Height - 1 );
    }
    #endregion
}