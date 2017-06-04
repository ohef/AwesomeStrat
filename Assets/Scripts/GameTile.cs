using System;
using Assets.General.DataStructures;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameTile : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public int CostOfTraversal;

    public void Start()
    {
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        ExecuteEvents.ExecuteHierarchy( transform.parent.gameObject, eventData, ExecuteEvents.pointerEnterHandler );
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        eventData.pointerPress = this.gameObject;
        ExecuteEvents.ExecuteHierarchy( transform.parent.gameObject, eventData, ExecuteEvents.pointerDownHandler );
    }
}