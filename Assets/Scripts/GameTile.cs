using System;
using Assets.General.DataStructures;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameTile : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public int CostOfTraversal;

    //MapDecorator decorator;

    public void Start()
    {
        //decorator = BattleSystem.Instance.Map.GetComponent<MapDecorator>();
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        ExecuteEvents.ExecuteHierarchy( transform.parent.gameObject, eventData, ExecuteEvents.pointerEnterHandler );
        //decorator.RenderForPath( new Vector2Int[] { BattleSystem.Instance.Map.TilePos[ this ] } );
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        ExecuteEvents.ExecuteHierarchy( transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler );
    }
}