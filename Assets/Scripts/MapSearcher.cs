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
    private static Dictionary<Vector2Int, GridNode> CalculateNodeMap( GameMap map, Vector2Int startPosition, int bound )
    {
        var toRet = new Dictionary<Vector2Int, GridNode>();

        foreach ( var pos in map.GetTilesWithinAbsoluteRange( startPosition, bound ) )
        {
            toRet.Add( pos, new GridNode( map.TilePos[ pos ], pos ) );
        }

        foreach ( var pos in map.GetTilesWithinAbsoluteRange( startPosition, bound ) )
        {
            toRet[ pos ].Neighbours = GetNeighborsCross( pos, map, toRet );
        }

        return toRet;
    }

    private static Dictionary<Vector2Int, GridNode> CalculateNodeMap( GameMap map )
    {
        var toRet = new Dictionary<Vector2Int, GridNode>();

        foreach ( var pos in map.AllMapPositions() )
        {
            toRet.Add( pos, new GridNode( map.TilePos[ pos ], pos ) );
        }

        foreach ( var pos in map.AllMapPositions() )
        {
            toRet[ pos ].Neighbours = GetNeighborsCross( pos, map, toRet );
        }
        return toRet;
    }

    private static List<GridNode> GetNeighborsCross( Vector2Int pos, GameMap map, Dictionary<Vector2Int, GridNode> graph )
    {
        var nodesToReturn = new List<GridNode>( 4 );
        foreach ( var direction in Vector2Int.AllDirections )
        {
            Vector2Int updatedDirection = pos + direction;

            GridNode val;
            bool existsInGraph = graph.TryGetValue( updatedDirection, out val );
            if ( existsInGraph == false ) continue;

            bool unitPresent = map.Occupied( updatedDirection );
            if ( unitPresent == true ) continue;

            //Assuming that i tile with a cost of 9999 is impassable, probably bad to do this but it works for now
            //It'll definitely need to be changed if you have different units that can cross or something
            //TODO: Don't do this?
            bool costWayHigh = map.TilePos[ updatedDirection ].CostOfTraversal > 1000;
            if ( costWayHigh == true ) continue;

            GridNode toAdd = graph[ updatedDirection ];
            nodesToReturn.Add( toAdd );
        }
        return nodesToReturn;
    }

    public static List<Vector2Int> Search( Vector2Int start, Vector2Int goal, GameMap map )
    {
        Dictionary<Vector2Int, GridNode> gridNodeMap = CalculateNodeMap( map );
        return Search( start, goal, gridNodeMap );
    }

    public static List<Vector2Int> Search( Vector2Int start, Vector2Int goal, GameMap map, int bound )
    {
        Dictionary<Vector2Int, GridNode> gridNodeMap = CalculateNodeMap( map, start, bound );
        return Search( start, goal, gridNodeMap, bound );
    }

    public static List<Vector2Int> ReachablePoints( Vector2Int start, GameMap map, int bound = int.MaxValue )
    {
        Dictionary<Vector2Int, GridNode> gridNodeMap = CalculateNodeMap( map, start, bound );

        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        Stack<GridNode> frontier = new Stack<GridNode>();

        GridNode startNode = gridNodeMap[ start ];

        startNode.pCost = 0;
        frontier.Push( startNode );

        while ( frontier.Count != 0 )
        {
            var currentNode = frontier.Pop();

            closedSet.Add( currentNode );

            foreach ( GridNode neighbour in currentNode.Neighbours )
            {
                int tempPCost = currentNode.pCost + neighbour.Cost;

                if ( tempPCost > bound )
                    continue;

                if ( !frontier.Contains( neighbour ) )
                    frontier.Push( neighbour );
                else if ( tempPCost >= neighbour.pCost )
                    continue;

                neighbour.pCost = tempPCost;
            }
        }
        return closedSet.Select( node => node.Location ).ToList();
    }

    private static List<Vector2Int> Search( Vector2Int start, Vector2Int goal, Dictionary<Vector2Int, GridNode> gridNodeMap, int bound = int.MaxValue )
    {
        if ( start == goal )
            return new List<Vector2Int> { goal };

        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        ModifiableBinaryHeap<GridNode> frontier = new ModifiableBinaryHeap<GridNode>();

        GridNode startNode = gridNodeMap[ start ];
        GridNode goalNode = gridNodeMap[ goal ];

        startNode.pCost = 0;
        startNode.hCost = GridNode.CalculateHeuristic( startNode, goalNode );

        frontier.Push( startNode );

        while ( frontier.Count != 0 )
        {
            var currentNode = frontier.Pop();
            if ( currentNode == goalNode )
                return ReconstructPath( currentNode, startNode );

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

    private static List<Vector2Int> ReconstructPath( GridNode finalNode, GridNode startNode )
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
}

internal class GridNode : IComparable<GridNode>, IEqualityComparer<GridNode>
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
