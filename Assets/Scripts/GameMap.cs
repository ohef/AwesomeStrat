using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System.Linq;

namespace Assets.Map
{
    [RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
    public class GameMap : MonoBehaviour
    {
        public int Width;
        public int Height;

        private Mesh m_MapMesh;
        public Unit DefaultUnit;
        public GameTile TilePrefab;
        public GameTile[,] m_TileMap;
        public Transform ObjectOffset;

        public Material NormalMat;
        public Material MovementMat;
        public Material AttackRangeMat;
        public Material SelectionMat;

        #region Monobehaviour Functions

        void Awake()
        {
            InitializeMap();

            this.GetComponent<MeshFilter>().mesh = m_MapMesh = CreateGridMesh( Width, Height );
            m_MapMesh.subMeshCount = 4;

            this.GetComponent<MeshRenderer>().materials = new Material[]
                {
                NormalMat,
                MovementMat,
                AttackRangeMat,
                SelectionMat,
                };

            var collider = this.GetComponent<BoxCollider>();
            collider.size = new Vector3( Width, 0, Height );
            collider.center = collider.size * 0.5f;

            InstantiateDefaultUnit( new Vector2Int( 0, 0 ) );
        }

        private Unit InstantiateDefaultUnit( Vector2Int v )
        {
            var unit = Instantiate( DefaultUnit );
            this[ unit.Position = v ].UnitOccupying = unit;
            unit.transform.SetParent( ObjectOffset );
            unit.transform.localPosition = v.ToVector3();
            return unit;
        }

        void Start()
        {
        }

        #endregion

        #region Member Functions

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

        public IEnumerable<Vector2Int> GetValidMovementPositions( Unit unit )
        {
            return GetTilesWithinAbsoluteRange( unit.Position, unit.Movement )
                .Where( position => MapSearcher.Search( this[ unit.Position ], this[ position ], this.m_TileMap, unit.Movement ) != null );
        }

        // Function that renders where a unit can move
        public void RenderUnitMovement( Unit unit, float alpha = 1.0f )
        {
            List<Vector2Int> validMovementTiles = GetValidMovementPositions( unit ).ToList();

            m_MapMesh.SetTriangles(
                validMovementTiles.SelectMany<Vector2Int, int>( TrianglesForPosition ).ToList(), 1 );
            m_MapMesh.SetTriangles(
                GetFringeAttackTiles( new HashSet<Vector2Int>( validMovementTiles ), unit.AttackRange )
                .SelectMany<Vector2Int, int>( TrianglesForPosition )
                .ToList(), 2 );

            Func<Material, Color> switchAlpha = mat =>
            {
                var temp = mat.GetColor( "_Color" );
                temp.a = alpha;
                return temp;
            };

            AttackRangeMat.SetColor( "_Color", switchAlpha( AttackRangeMat ) );
            MovementMat.SetColor( "_Color", switchAlpha( MovementMat ) );
        }

        public void RenderSelection( IEnumerable<Vector2Int> tiles )
        {
            m_MapMesh.SetTriangles( tiles.SelectMany( tile => TrianglesForPosition( tile.x, tile.y ) ).ToList(), 3 );
        }

        public void StopRenderingOverlays()
        {
            m_MapMesh.SetTriangles( new int[] { }, 1 );
            m_MapMesh.SetTriangles( new int[] { }, 2 );
            m_MapMesh.SetTriangles( new int[] { }, 3 );
        }

        private HashSet<Vector2Int> GetAttackTiles( HashSet<Vector2Int> movementTiles, int attackRange )
        {
            var temp = GetFringeAttackTiles( movementTiles, attackRange );
            temp.IntersectWith( movementTiles );
            return temp;
        }

        private HashSet<Vector2Int> GetFringeAttackTiles( HashSet<Vector2Int> movementTiles, int attackRange )
        {
            HashSet<Vector2Int> attackTiles = new HashSet<Vector2Int>();

            foreach ( Vector2Int tile in movementTiles )
                foreach ( Vector2Int direction in new Vector2Int[] { Vector2Int.Up, Vector2Int.Down, Vector2Int.Left, Vector2Int.Right } )
                    foreach ( int coef in Enumerable.Range( 1, attackRange ) )
                    {
                        Vector2Int neighbour = ClampWithinMap( tile + direction * coef );
                        if ( movementTiles.Contains( neighbour ) == false )
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
                    gameTile.transform.SetParent( ObjectOffset );
                    gameTile.transform.localPosition = gameTile.Position.ToVector3();
                    gameTile.name = gameTile.Position.ToString();
                }
        }

        public GameTile this[ int x, int y ]
        {
            get { return m_TileMap[ x, y ]; }
            set { m_TileMap[ x, y ] = value; }
        }

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
}