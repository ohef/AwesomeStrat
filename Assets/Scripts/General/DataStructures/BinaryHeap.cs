using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.General.DataStructures
{
    public class BinaryHeap<T> : IHeap<T> where T : IComparable<T>
    {
        protected List<T> m_Storage;
        protected bool isMinHeap;

        public int Count
        {
            get { return m_Storage.Count; }
        }

        public BinaryHeap( bool isMinHeap = true )
        {
            this.isMinHeap = true;
            m_Storage = new List<T>();
            m_Storage.Clear();
        }

        protected virtual void Swap( int aIndex, int bIndex )
        {
            T bTemp = m_Storage[ bIndex ];
            m_Storage[ bIndex ] = m_Storage[ aIndex ];
            m_Storage[ aIndex ] = bTemp;
        }

        protected int GetParentIndex( int i )
        {
            i -= i % 2;
            i /= 2;
            return i;
        }

        protected int GetLeftChildIndex( int parentIndex )
        {
            return parentIndex * 2 + 1;
        }

        protected int GetRightChildIndex( int parentIndex )
        {
            return parentIndex * 2 + 2;
        }

        protected int BubbleUp( int parentIndex, int childIndex )
        {
            while ( true )
            {
                int result = m_Storage[ childIndex ].CompareTo( m_Storage[ parentIndex ] );
                if ( result < 0 && isMinHeap || result > 0 && !isMinHeap )
                {
                    Swap( parentIndex, childIndex );
                    int newParent = GetParentIndex( parentIndex );
                    childIndex = parentIndex;
                    parentIndex = newParent;
                }
                else break;
            }
            return childIndex;
        }

        protected void BubbleUpRecurse( int parentIndex, int childIndex )
        {
            int result = m_Storage[ childIndex ].CompareTo( m_Storage[ parentIndex ] );
            if ( result < 0 && isMinHeap || result > 0 && !isMinHeap )
            {
                Swap( parentIndex, childIndex );
                int newParent = GetParentIndex( parentIndex );
                BubbleUpRecurse( newParent, parentIndex );
            }
        }

        protected int BubbleDown( int i )
        {
            while ( true )
            {
                int lefti = GetLeftChildIndex( i );
                int righti = GetRightChildIndex( i );
                int swapIndex = 0;

                if ( lefti < m_Storage.Count )
                {
                    swapIndex = lefti;
                    if ( righti < m_Storage.Count )
                        swapIndex = m_Storage[ righti ].CompareTo( m_Storage[ lefti ] ) < 0 ? righti : swapIndex;

                    if ( m_Storage[ swapIndex ].CompareTo( m_Storage[ i ] ) < 0 )
                    {
                        Swap( swapIndex, i );
                        i = swapIndex;
                    }
                    else break;
                }
                else break;
            }

            return i;
        }

        protected void BubbleDownRecurse( int i )
        {
            int zeroCount = m_Storage.Count - 1;
            int lefti = GetLeftChildIndex( i );
            int righti = GetRightChildIndex( i );

            if ( lefti <= zeroCount )
            {
                int compResult = m_Storage[ lefti ].CompareTo( m_Storage[ i ] );
                if ( compResult < 0 && isMinHeap || compResult > 0 && !isMinHeap )
                {
                    Swap( lefti, i );
                    BubbleDownRecurse( lefti );
                }
            }

            if ( righti <= zeroCount )
            {
                int compResult = m_Storage[ righti ].CompareTo( m_Storage[ i ] );
                if ( compResult < 0 && isMinHeap || compResult > 0 && !isMinHeap )
                {
                    Swap( righti, i );
                    BubbleDownRecurse( righti );
                }
            }
        }

        public virtual void Push( T elem )
        {
            m_Storage.Add( elem );
            int child = m_Storage.Count - 1;
            int parent = GetParentIndex( child );
            BubbleUp( parent, child );
        }

        public virtual T Pop()
        {
            if ( m_Storage.Count == 0 )
                return default( T );
            T toRet = m_Storage[ 0 ];
            Swap( 0, m_Storage.Count - 1 );
            m_Storage.RemoveAt( m_Storage.Count - 1 );
            BubbleDown( 0 );
            return toRet;
        }
    }
}
