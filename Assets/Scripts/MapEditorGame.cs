using UnityEngine;
using System.Collections;
using Assets.Map;
using Assets.General.DataStructures;

public class MapEditorGame : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
        if ( Physics.Raycast( ray, out hit ) )
        {
            var obj = hit.transform.gameObject;
            if(obj.tag == "Map")
            {
                var map = obj.GetComponent<GameMap>();
                map.RenderUnitMovement( 
                    new Unit {
                        AttackRange = 1,
                        Movement = 2,
                        Position = map.MapInternal.ClampPositionViaMap( 
                            new Vector2Int( ( int )Mathf.Floor( hit.point.x ), ( int )Mathf.Floor( hit.point.z ) ) )
                    } );
            }
        }

    }
}
