using UnityEngine;
using System.Collections;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.Map;

[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
public class CursorControl : MonoBehaviour {

    private enum CursorState
    {
        Moving,
        Stationary
    }
    private CursorState state = CursorState.Stationary;
    private MapCursor internalCursor;

    public GameMap map;

    public delegate void CursorMovedHandler( Vector3 oldPosition, Vector3 newPositon );
    public event CursorMovedHandler CursorMoved;

    private Vector3 CursorPosition
    {
        get { return transform.position; }
        set
        {
            var oldposition = transform.position;
            transform.position = value;
            CursorMoved( oldposition, transform.position);
        }
    }

    public Vector2Int GridPosition { get { return new Vector2Int( ( int )CursorPosition.x, ( int )CursorPosition.z ); } }

    #region UnityMonoBehaviourFunctions

    void Awake()
    {
        internalCursor = new MapCursor( map.MapInternal );
    }

    void Start() { }

    void Update()
    {
        //assuming button input?
        var direction = new Vector2Int( ( int )Input.GetAxisRaw( "Horizontal" ), ( int )Input.GetAxisRaw( "Vertical" ) );

        if ( direction.ManhattanNorm() > 0 )
            switch ( state )
            {
                case CursorState.Stationary:
                    state = CursorState.Moving;
                    StartCoroutine( MoveCursorDiscreteAnim( internalCursor.MoveCursor( internalCursor.currentLocation + direction ) ) );
                    map.RenderUnitMovement( new Unit { Position = map.MapInternal[ internalCursor.currentLocation ].Position, AttackRange = 1, Movement = 4 } );
                    break;
                case CursorState.Moving:
                    break;
            }
    }
    #endregion

    IEnumerator MoveCursorDiscreteAnim( Vector2Int to )
    {
        Vector3 oldPosition = CursorPosition;
        Vector3 updatedPosition = new Vector3( to.x + 0.5f, oldPosition.y, to.y + 0.5f );

        for ( float i = 0 ; i < 0.99f ; i+=0.33f )
        {
            CursorPosition = new Vector3(
            Mathf.Lerp( oldPosition.x, updatedPosition.x, i ),
            oldPosition.y,
            Mathf.Lerp( oldPosition.z, updatedPosition.z, i ) );
            yield return null; 
        }

        CursorPosition = updatedPosition;

        state = CursorState.Stationary;
        yield return null;
    }

    private Mesh CreatePyramidMesh()
    {
        Vector3[] vertices = new Vector3[] { Vector3.zero, Vector3.forward, Vector3.forward + Vector3.right, Vector3.right, Vector3.up + Vector3.forward * 0.5f + Vector3.right * 0.5f };

        for ( int i = 0 ; i < vertices.Length ; i++ )
        {
            vertices[ i ] = Matrix4x4.Scale( new Vector3( 0.5f, -0.5f, 0.5f ) ) * ( vertices[ i ] + new Vector3( -0.5f, 0, -0.5f ) );
        }

        int[] triangles = new int[]
        {
            //bottom
            0,1,2,
            0,2,3,
            //sides
            0,1,4,
            1,2,4,
            2,3,4,
            3,0,4,
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //GetComponent<MeshRenderer>().materials = new Material[] { Resources.Load( "New Material 1" ) as Material, };

        return mesh;
    }
}
