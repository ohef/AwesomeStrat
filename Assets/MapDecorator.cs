using Assets.General.DataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public abstract class MapDecorator : MonoBehaviour
{
    public abstract void RenderForPath( IEnumerable<Vector2Int> path );
    public abstract void ShowUnitMovement( Unit unit );
}