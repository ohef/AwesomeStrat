using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.General.DataStructures;

public interface IHeuristic<TNode, THeurVal>
{
    THeurVal CalculateHeuristic( TNode a, TNode b );
}

public class MapSearcher
{
    public static void CalculateNodeMap( GameMap map, GridNode[,] gridNodeMap )
    {
        foreach ( var pos in map.AllMapPositions() )
        {
            gridNodeMap[ pos.x, pos.y ] = new GridNode( map.TilePos[ pos ], pos );
        }

        foreach ( var pos in map.AllMapPositions() )
        {
            gridNodeMap[ pos.x, pos.y ].Neighbours = GetNeighborsCross( pos, map, gridNodeMap );
        }
    }

    public static List<Vector2Int> Search( Vector2Int start, Vector2Int goal, GameMap map, int bound = int.MaxValue )
    {
        if ( start == goal )
            return new List<Vector2Int> { goal };

        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        ModifiableBinaryHeap<GridNode> frontier = new ModifiableBinaryHeap<GridNode>();

        GridNode[,] gridNodeMap = new GridNode[ map.Width, map.Height ];
        CalculateNodeMap( map, gridNodeMap );

        GridNode startNode = gridNodeMap[ start.x, start.y ];
        GridNode goalNode = gridNodeMap[ goal.x, goal.y ];

        startNode.pCost = 0;
        startNode.hCost = GridNode.CalculateHeuristic( startNode, goalNode );

        frontier.Push( startNode );

        while ( frontier.Count != 0 )
        {
            var currentNode = frontier.Pop();
            if ( currentNode == goalNode )
                return ReconstructPath( currentNode, startNode, map );

            closedSet.Add( currentNode );

            foreach ( GridNode neighbour in currentNode.Neighbours )
            {
                if ( closedSet.Contains( neighbour ) )
                    continue;
                int tempPCost = currentNode.pCost + neighbour.Cost;
                if ( tempPCost > bound )
                    continue;

                if ( !frontier.Contains( neighbour ) )
                    frontier.Push( neighbour );
                else if ( tempPCost >= neighbour.pCost )
                    continue;

                neighbour.Parent = currentNode;
                neighbour.pCost = tempPCost;
                neighbour.hCost = GridNode.CalculateHeuristic( neighbour, goalNode );
                frontier.ValueChanged( neighbour );
            }
        }

        //There is no path
        return null;
    }

    public static List<Vector2Int> ReconstructPath( GridNode finalNode, GridNode startNode, GameMap map )
    {
        var path = new List<Vector2Int>();
        GridNode temp = finalNode;
        while ( true )
        {
            if ( temp == startNode )
                break;
            path.Add( temp.Location );
            temp = temp.Parent;
        }

        path.Add( temp.Location );
        path.Reverse();
        return path;
    }

    //private static List<GridNode> GetNeighborsCross( GridNode pivotNode, GridNode[,] map )
    //{
    //    var nodesToReturn = new List<GridNode>( 4 );
    //    Vector2Int[] directions = { new Vector2Int( 0, 1 ), new Vector2Int( 1, 0 ), new Vector2Int( -1, 0 ), new Vector2Int( 0, -1 ) };
    //    foreach ( var direction in directions )
    //    {
    //        if ( pivotNode.Location.x + direction.x > map.GetLength( 0 ) - 1 ||
    //            pivotNode.Location.x + direction.x < 0 ||
    //            pivotNode.Location.y + direction.y > map.GetLength( 1 ) - 1 ||
    //            pivotNode.Location.y + direction.y < 0 )
    //        {
    //            continue;
    //        }

    //        var toAdd = map[ pivotNode.Location.x + direction.x, pivotNode.Location.y + direction.y ];
    //        nodesToReturn.Add( toAdd );
    //    }
    //    return nodesToReturn;
    //}

    private static List<GridNode> GetNeighborsCross( Vector2Int pos, GameMap map, GridNode[,] graph )
    {
        var nodesToReturn = new List<GridNode>( 4 );
        Vector2Int[] directions = { new Vector2Int( 0, 1 ), new Vector2Int( 1, 0 ), new Vector2Int( -1, 0 ), new Vector2Int( 0, -1 ) };
        foreach ( var direction in directions )
        {
            Vector2Int updatedDirection = pos + direction;
            bool outOfBounds = map.IsOutOfBounds( updatedDirection );
            bool unitPresent = map.Occupied( updatedDirection );
            if ( outOfBounds || unitPresent )
            {
                continue;
            }

            GridNode toAdd = graph[ updatedDirection.x, updatedDirection.y ];
            nodesToReturn.Add( toAdd );
        }
        return nodesToReturn;
    }

    private static List<GridNode> GetNeighbors( GridNode pivotNode, GridNode[,] map )
    {
        var nodesToReturn = new List<GridNode>( 8 );
        for ( int i = -1 ; i <= 1 ; i++ )
            for ( int j = -1 ; j <= 1 ; j++ )
            {
                if ( i == 0 && j == 0 ||
                    pivotNode.Location.x + i > map.GetLength( 0 ) ||
                    pivotNode.Location.x + i < 0 ||
                    pivotNode.Location.y + j > map.GetLength( 1 ) ||
                    pivotNode.Location.y + j < 0 )
                {
                    continue;
                }

                var toAdd = map[ pivotNode.Location.x + i, pivotNode.Location.y + j ];
                nodesToReturn.Add( toAdd );
            }
        return nodesToReturn;
    }
}

public class GridNode : IComparable<GridNode>, IEqualityComparer<GridNode>
{
    public int Cost;
    public int fCost
    {
        get { return pCost + hCost; }
    }

    public GridNode Parent;

    public Vector2Int Location;

    public IEnumerable<GridNode> Neighbours { get; set; }

    public int pCost = 0;
    public int hCost = 0;

    public GridNode( GameTile tile, Vector2Int location )
    {
        Location = location;
        Cost = tile.CostOfTraversal;
    }

    public int CompareTo( GridNode other )
    {
        if ( this.fCost < other.fCost )
            return -1;
        else if ( this.fCost > other.fCost )
            return 1;
        else
            return 0;
    }

    public static int CalculateHeuristic( GridNode a, GridNode b )
    {
        Vector2Int distance = ( b.Location - a.Location );
        return Math.Abs( distance.x ) + Math.Abs( distance.y );
    }

    public bool Equals( GridNode x, GridNode y )
    {
        return x.Location.x == y.Location.x && x.Location.y == y.Location.y;
    }

    public int GetHashCode( GridNode obj )
    {
        return obj.Location.x + obj.Location.y << 16;
    }
}
