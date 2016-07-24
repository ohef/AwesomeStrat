using UnityEngine;
using System.Collections;
using Assets.Map;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using UnityEngine.EventSystems;

public class MapEditorGame : MonoBehaviour
{
    public GameObject cursorLook;

    public enum MapCursorState
    {
        PlacingTile,
        PlacingUnit
    };

    public enum SelectionState
    {
        Multiple,
        Single,
    };

    MapCursorState mapCursorState = MapCursorState.PlacingTile;

    void Awake()
    {
    }

    // Use this for initialization
    void Start()
    {
        cursorLook = Instantiate( cursorLook );
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
        if ( Physics.Raycast( ray, out hit ) )
        {
            var map = hit.transform.gameObject.tag == "Map" ? hit.transform.GetComponent<GameMap>() : null;
            if ( map != null )
            {
                switch ( mapCursorState )
                {
                    case MapCursorState.PlacingTile:
                        hit.point = new Vector3( Mathf.Floor( hit.point.x ), hit.point.y, Mathf.Floor( hit.point.z ) );
                        cursorLook.transform.position = map.MapInternal.ClampWithinMapViaXZPlane( hit.point ) + new Vector3( 0.5f, 0, 0.5f );
                        //cursorLook.transform.position = map.MapInternal.ClampPositionViaMap(
                        //    new Vector2Int(
                        //        ( int )Mathf.Floor( hit.point.x ),
                        //        ( int )Mathf.Floor( hit.point.z ) ) )
                        //        .ToVector3( Vector2IntExtensions.Axis.Y ) + new Vector3( 0.5f, 0, 0.5f );
                        break;
                    default:
                        break;
                }
                //    var map = obj.GetComponent<GameMap>();
                //    map.RenderUnitMovement( 
                //        new Unit {
                //            AttackRange = 1,
                //            Movement = 2,
                //            Position = map.MapInternal.ClampPositionViaMap( 
                //                new Vector2Int( ( int )Mathf.Floor( hit.point.x ), ( int )Mathf.Floor( hit.point.z ) ) )
                //        } );
                //}
                //}
            }
        }
    }
}