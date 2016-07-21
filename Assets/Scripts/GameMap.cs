using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Map;
using System.Linq;

[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
//[ExecuteInEditMode]
public class GameMap : MonoBehaviour
{
    private Map m_MapInternal;
    public Map MapInternal { get { return m_MapInternal; } }

    private Mesh m_MapMesh;
    public GameTile tilePrefab;

    private Material MovementMat;
    private Material AttackRangeMat;

    #region Monobehaviour Functions

    void Awake()
    {
        MovementMat = Resources.Load( "MovementMaterial" ) as Material;
        AttackRangeMat = Resources.Load( "AttackRange" ) as Material;
        m_MapInternal = new Map( 10, 10, tilePrefab.tileData);

        foreach ( Tile tile in m_MapInternal )
        {
            var gameTile = Instantiate( tilePrefab );
            gameTile.tileData = tile;
        }

        this.GetComponent<MeshFilter>().mesh = m_MapMesh = CreateGridMesh( m_MapInternal.MapSize.x, m_MapInternal.MapSize.y );
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    #endregion

    #region Member Functions
    public void InstantiateMapData( int mapSizeX, int mapSizeY)
    {
        m_MapInternal = new Map( mapSizeX, mapSizeY );
    }

    private void AddTrianglesForPosition(int i, int j, List<int> triangleList)
    {
        int height = m_MapInternal.MapSize.y;
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

    // Function that renders where a unit can move
    public void RenderUnitMovement( Unit unit )
    {
        m_MapMesh.subMeshCount = 3;
        List<int> MovementSet = new List<int>();
        List<int> AttackSet = new List<int>();

        {
            HashSet<Vector2Int> movementTiles = new HashSet<Vector2Int>();

            Func<int, int> clampX = i => m_MapInternal.ClampX( i );
            Func<int, int> clampY = i => m_MapInternal.ClampY( i );

            for ( int i = clampX( unit.Position.x - unit.Movement ) ; i <= clampX( unit.Position.x + unit.Movement ) ; i++ )
                for ( int j = clampY( unit.Position.y - unit.Movement ) ; j <= clampY( unit.Position.y + unit.Movement ) ; j++ )
                {
                    if ( ( unit.Position - new Vector2Int( i, j ) ).ManhattanNorm() <= unit.Movement )
                    {
                        List<Tile> result = MapSearcher.Search( MapInternal[ unit.Position ], MapInternal[ i, j ], MapInternal );

                        int totalCost = 0;
                        foreach ( var tile in result )
                            totalCost += tile.CostOfTraversal;

                        if ( totalCost <= unit.Movement )
                        {
                            AddTrianglesForPosition( i, j, MovementSet );
                            movementTiles.Add( new Vector2Int( i, j ) );
                        }
                    }
                }

            foreach ( var attackTile in GetAttackTiles(movementTiles, unit.AttackRange ))
            {
                AddTrianglesForPosition( attackTile.x, attackTile.y, AttackSet );
            }
        }

        m_MapMesh.SetTriangles( MovementSet, 1 );
        m_MapMesh.SetTriangles( AttackSet, 2 );

        this.GetComponent<MeshRenderer>().materials = new Material[]
            {
                Resources.Load( "New Material 1" ) as Material,
                MovementMat,
                AttackRangeMat,
            };
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
                    Vector2Int neighbour = m_MapInternal.ClampPositionViaMap( tile + direction * attackRange );
                    if ( movementTiles.Contains( neighbour ) == false )
                        attackTiles.Add( neighbour );
                }
            }
        }
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

        for ( int i = 0 ; i < width; i++ )
            for ( int j = 0 ; j < height; j++ )
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
                triangles[ j + 3 ] = i ;
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
}