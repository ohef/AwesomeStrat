using Assets.General.DataStructures;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameTile : MonoBehaviour, IPointerEnterHandler
{
    public int CostOfTraversal;

    MapDecorator decorator;

    public void Start()
    {
        decorator = BattleSystem.Instance.Map.GetComponent<MapDecorator>();
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        decorator.RenderForPath( new Vector2Int[] { BattleSystem.Instance.Map.TilePos[ this ] } );
    }
}