using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.General.DataStructures;

namespace Assets.Map
{
    public interface IHeuristic<TNode, THeurVal>
    {
        THeurVal CalculateHeuristic( TNode a, TNode b );
    }

    public class MapSearcher
    {
        public static void CopyIntoGridNodeMap( GameTile[,] map, GridNode[,] gridNodeMap )
        {
            for ( int i = 0 ; i < map.GetLength( 0 ) ; i++ )
                for ( int j = 0 ; j < map.GetLength( 1 ) ; j++ )
                {
                    gridNodeMap[ i, j ] = new GridNode( map[ i, j ] );
                }
        }

        public static List<GameTile> Search( GameTile start, GameTile goal, GameTile[,] map, int bound = int.MaxValue )
        {
            HashSet<GridNode> closedSet = new HashSet<GridNode>();
            ModifiableBinaryHeap<GridNode> frontier = new ModifiableBinaryHeap<GridNode>();

            GridNode[,] gridNodeMap = new GridNode[ map.GetLength( 0 ), map.GetLength( 1 ) ];
            CopyIntoGridNodeMap( map, gridNodeMap );

            GridNode startNode = gridNodeMap[ start.Position.x, start.Position.y ];
            GridNode goalNode = gridNodeMap[ goal.Position.x, goal.Position.y ];

            startNode.pCost = 0;
            startNode.hCost = GridNode.CalculateHeuristic( startNode, goalNode );

            frontier.Push( startNode );

            while ( frontier.Count != 0 )
            {
                var currentNode = frontier.Pop();
                if ( currentNode == goalNode )
                    return ReconstructPath( currentNode, startNode );

                closedSet.Add( currentNode );

                foreach ( GridNode neighbour in GetNeighborsCross( currentNode, gridNodeMap ) )
                {
                    if ( closedSet.Contains( neighbour ) )
                        continue;
                    int tempPCost = currentNode.pCost + neighbour.Tile.CostOfTraversal;
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

            return null;
        }

        public static List<GameTile> ReconstructPath( GridNode finalNode, GridNode startNode )
        {
            var path = new List<GameTile>();
            GridNode temp = finalNode;
            while ( true )
            {
                if ( temp == startNode )
                    break;
                path.Add( temp.Tile );
                temp = temp.Parent;
            }

            path.Add( temp.Tile );
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

        public Vector2Int Location { get { return Tile.Position; } }

        public GameTile Tile
        {
            get
            {
                return m_Tile;
            }
        }

        public int pCost = 0;
        public int hCost = 0;

        private GameTile m_Tile;

        public GridNode( GameTile tile )
        {
            this.m_Tile = tile;
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
}
