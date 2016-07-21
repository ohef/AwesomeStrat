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
        public static void CopyIntoGridNodeMap( Map map, GridNode[,] gridNodeMap, int sizex, int sizey )
        {
            for ( int i = 0 ; i < sizex ; i++ )
                for ( int j = 0 ; j < sizey ; j++ )
                {
                    gridNodeMap[ i, j ] = new GridNode( map[ i, j ] );
                }
        }

        public static List<Tile> Search( Tile start, Tile goal, Map map )
        {
            HashSet<GridNode> closedSet = new HashSet<GridNode>();
            ModifiableBinaryHeap<GridNode> frontier = new ModifiableBinaryHeap<GridNode>();

            GridNode[,] gridNodeMap = new GridNode[ map.MapSize.x, map.MapSize.y ];
            CopyIntoGridNodeMap( map, gridNodeMap, map.MapSize.x, map.MapSize.y );

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

            return ReconstructPath( frontier.Pop(), frontier.Pop() );
        }

        public static List<Tile> ReconstructPath( GridNode finalNode, GridNode startNode )
        {
            var path = new List<Tile>();
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

    public class GridNode : HeapItem, IComparable<GridNode>
    {
        public int Cost;
        public int fCost
        {
            get { return pCost + hCost; }
        }

        public GridNode Parent;

        public Vector2Int Location { get { return Tile.Position; } }

        public Tile Tile
        {
            get
            {
                return m_Tile;
            }
        }

        public int pCost = 0;
        public int hCost = 0;

        private Tile m_Tile;

        public GridNode( Tile tile )
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
    }
}
