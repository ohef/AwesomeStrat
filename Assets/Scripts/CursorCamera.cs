using UnityEngine;
using System.Collections;

public class CursorCamera : MonoBehaviour {

    public CursorControl cursor;

    void Awake()
    {
        //cursor.CursorMoved += ( oldPos, newPos ) => RenderUnitMovement( new Unit { Movement = 2, Position = Cursor.GridPosition, AttackRange = 2 } );
        cursor.CursorMoved += UpdateCamera;
    }

	// Use this for initialization
	void Start () {
        this.transform.LookAt( cursor.transform );
    }

    // Update is called once per frame
    void Update () {
	
	}

    void UpdateCamera( Vector3 oldPosition, Vector3 newPositon )
    {
        this.transform.position = this.transform.position + ( newPositon - oldPosition );
    }
}
