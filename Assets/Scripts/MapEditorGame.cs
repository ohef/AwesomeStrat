using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Map;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using UnityEngine.EventSystems;
using System;

public class MapEditorGame : MonoBehaviour
{
    public GameObject cursorLook;
    public GameMap Map;

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
    SelectionState selectionState = SelectionState.Single;
    HashSet<Vector2Int> selectionCanvas;

    void Awake()
    {
        selectionCanvas = new HashSet<Vector2Int>();
    }

    // Use this for initialization
    void Start()
    {
        cursorLook = Instantiate( cursorLook );
    }

    // Update is called once per frame
    void Update()
    {
        Map.RenderSelection( selectionCanvas );
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
                        Vector2Int mapPoint = map.MapInternal.ClampWithinMap( new Vector2Int( ( int )hit.point.x, ( int )hit.point.z ) );
                        HandleClick(mapPoint);
                        cursorLook.transform.position = map.MapInternal.ClampWithinMapViaXZPlane( hit.point ) + new Vector3( 0.5f, 0, 0.5f );
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void HandleClick( Vector2Int clickedTile )
    {
        switch ( selectionState )
        {
            case SelectionState.Single:
                if ( Input.GetMouseButtonUp( 0 ) == true )
                {
                    selectionCanvas.Clear();
                    selectionCanvas.Add( clickedTile );
                }
                break;
            default:
                break;
        }
    }
}