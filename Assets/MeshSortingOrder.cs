using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshSortingOrder : MonoBehaviour
{

    public string layerName;
    public int order;

    private MeshRenderer rend;
    void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        rend.sortingLayerName = layerName;
        rend.sortingOrder = order;
    }

    public void Update()
    {
        if ( rend.sortingLayerName != layerName )
            rend.sortingLayerName = layerName;
        if ( rend.sortingOrder != order )
            rend.sortingOrder = order;
    }

    public void OnValidate()
    {
        rend = GetComponent<MeshRenderer>();
        rend.sortingLayerName = layerName;
        rend.sortingOrder = order;
    }
}
