using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Map;
using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

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

    private MapCursorState mapCursorState = MapCursorState.PlacingTile;
    private SelectionState selectionState = SelectionState.Single;
    private HashSet<Vector2Int> selectionCanvas;
    private GameObject cursorLabel;

    void Awake()
    {
        selectionCanvas = new HashSet<Vector2Int>();
        cursorLabel = new GameObject();
        cursorLabel.AddComponent<CanvasRenderer>();
        cursorLabel.AddComponent<Text>();
        var text = cursorLabel.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        var rectTrans = cursorLabel.GetComponent<RectTransform>();
        rectTrans.SetParent( FindObjectOfType<Canvas>().transform );
        rectTrans.transform.localScale = Vector3.one;
        cursorLabel.layer = 5;
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
                Vector2Int mapPoint = map.MapInternal.ClampWithinMap( new Vector2Int( ( int )hit.point.x, ( int )hit.point.z ) );
                cursorLabel.transform.position = Input.mousePosition + Vector3.forward * 10;
                cursorLabel.GetComponent<Text>().text = string.Format( "Map Position: {0}\n MovementCost: {1}", mapPoint, map.MapInternal[ mapPoint ].CostOfTraversal );
                switch ( mapCursorState )
                {
                    case MapCursorState.PlacingTile:
                        hit.point = new Vector3( Mathf.Floor( hit.point.x ), hit.point.y, Mathf.Floor( hit.point.z ) );
                        HandleClick( mapPoint );
                        cursorLook.transform.position = map.MapInternal.ClampWithinMapViaXZPlane( hit.point ) + new Vector3( 0.5f, 0, 0.5f );
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void SwitchSelection( bool switchState )
    {
        selectionState = switchState == true ? SelectionState.Multiple : SelectionState.Single;
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
            case SelectionState.Multiple:
                if ( Input.GetMouseButtonUp( 0 ) == true )
                {
                    selectionCanvas.Add( clickedTile );
                }
                break;
            default:
                break;
        }
    }
}