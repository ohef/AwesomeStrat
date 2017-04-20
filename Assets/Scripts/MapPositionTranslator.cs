using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MapPositionTranslator
{
    public const Vector2IntExt.Axis DefaultAxis = Vector2IntExt.Axis.Z;
    public static Vector3 GetTransform( Vector2Int pos )
    {
        return pos.ToVector3( DefaultAxis );
    }
}