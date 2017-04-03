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
        for ( int i = 0 ; i < map.Width ; i++ )
            for ( int j = 0 ; j < map.Height ; j++ )
            {
                gridNodeMap[ i, j ] = new GridNode( map[ i, j ] );

                if ( map.Occupied( map[ i, j ] ) )
                    //So we set the cost to be half of a max int because it WRAPS AROUND to negative when added to pCost. 
                    //It's probably better to have a unit or impassable terrain logic somewhere else and leave the searcher simple;
                    //Lest we populate a general class.
                    gridNodeMap[ i, j ].Cost = 9999;
            }
    }

    public static List<GameTile> Search( GameTile start, GameTile goal, GameMap map, int bound = int.MaxValue )
    {
        if ( start == goal )
            return new List<GameTile> { goal };

        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        ModifiableBinaryHeap<GridNode> frontier = new ModifiableBinaryHeap<GridNode>();

        GridNode[,] gridNodeMap = new GridNode[ map.Width, map.Height ];
        CalculateNodeMap( map, gridNodeMap );

        GridNode startNode = gridNodeMap[ start.Position.x, start.Position.y ];
        GridNode goalNode = gridNodeMap[ goal.Position.x, goal.Position.y ];

        startNode.pCost = 0;
        startNode.hCost = GridNode.CalculateHeuristic( startNode, goalNode );

        frontier.Push( startNode );

        while ( frontier.Count != 0 )
        {
            var currentNode = frontier.Pop();
            if ( currentNode == goalNode )
                return ReconstructPath( currentNode, startNode, map );

            closedSet.Add( currentNode );

            foreach ( GridNode neighbour in GetNeighborsCross( currentNode, gridNodeMap ) )
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

    public static List<GameTile> ReconstructPath( GridNode finalNode, GridNode startNode, GameMap map )
    {
        var path = new List<GameTile>();
        GridNode temp = finalNode;
        while ( true )
        {
            if ( temp == startNode )
                break;
            path.Add( map[ temp.Location ] );
            temp = temp.Parent;
        }

        path.Add( map[ temp.Location ] );
        path.Reverse();
        return path;
    }

    private static List<GridNode> GetNeighborsCross( GridNode pivotNode, GridNode[,] map )
    {
        var nodesToReturn = new List<GridNode>( 4 );
        Vector2Int[] directions = { new Vector2Int( 0, 1 ), new Vector2Int( 1, 0 ), new Vector2Int( -1, 0 ), new Vector2Int( 0, -1 ) };
        foreach ( var direction in directions )
        {
            if ( pivotNode.Location.x + direction.x > map.GetLength( 0 ) - 1 ||
                pivotNode.Location.x + direction.x < 0 ||
                pivotNode.Location.y + direction.y > map.GetLength( 1 ) - 1 ||
                pivotNode.Location.y + direction.y < 0 )
            {
                continue;
            }

            var toAdd = map[ pivotNode.Location.x + direction.x, pivotNode.Location.y + direction.y ];
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

    public int pCost = 0;
    public int hCost = 0;

    public GridNode( GameTile tile )
    {
        Location = tile.Position;
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
        return obj.Location.x + obj.Location.y * 1000;
    }
}
