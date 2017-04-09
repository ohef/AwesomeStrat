using Assets.General.DataStructures;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapData : ScriptableObject, IEnumerable<GameTile>
{
    public int Width;
    public int Height;
    public Unit[] UnitPlacement;
    public GameTile[] GameTiles;

    public GameTile this[ int x, int y ]
    {
        get { return GameTiles[ x * Width + y ]; }
        set { GameTiles[ x * Width + y ] = value; }
    }

    public GameTile GetTileAtPosition( int x, int y )
    {
        return GameTiles[ x * Width + y ];
    }

    public void SetTileAtPosition( int x, int y, GameTile value )
    {
        GameTiles[ x * Width + y ] = value;
    }

    public void SetUnitAtPosition( int x, int y, Unit value )
    {
        UnitPlacement[ x * Width + y ] = value;
    }

    public Unit GetUnitAtPosition( int x, int y )
    {
        return UnitPlacement[ x * Width + y ];
    }

    public static MapData CreateMapData( int Width, int Height )
    {
        MapData toret = ScriptableObject.CreateInstance<MapData>();
        toret.GameTiles = new GameTile[ Width * Height ];
        toret.UnitPlacement = new Unit[ Width * Height ];
        toret.Width = Width;
        toret.Height = Height;
        AssetDatabase.CreateAsset( toret, "Assets/Maps/NewMap.asset" );
        AssetDatabase.SaveAssets();
        return toret;
    }

    public IEnumerator<GameTile> GetEnumerator()
    {
        return GameTiles.GetEnumerator() as IEnumerator<GameTile>;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GameTiles.GetEnumerator();
    }
}