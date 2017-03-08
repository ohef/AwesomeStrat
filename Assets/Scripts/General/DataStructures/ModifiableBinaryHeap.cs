using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Assets.General.DataStructures
{
    public class ModifiableBinaryHeap<T> : BinaryHeap<T>, IContains<T> where T : IComparable<T>
    {
        private Dictionary<T, int> BookKeeper;

        public ModifiableBinaryHeap( bool isMinHeap = true ) : base( isMinHeap )
        {
            BookKeeper = new Dictionary<T, int>();
        }

        public void ValueChanged( T item )
        {
            if ( BookKeeper[ item ] > 0 || BookKeeper[ item ] < m_Storage.Count )
            {
                BookKeeper[ item ] = BubbleUp( GetParentIndex( BookKeeper[ item ] ), BookKeeper[ item ] );
                BookKeeper[ item ] = BubbleDown( BookKeeper[ item ] );
            }
        }

        protected override void Swap( int aIndex, int bIndex )
        {
            BookKeeper[ m_Storage[ aIndex ] ] = bIndex;
            BookKeeper[ m_Storage[ bIndex ] ] = aIndex;
            base.Swap( aIndex, bIndex );
        }

        public override void Push( T elem )
        {
            m_Storage.Add( elem );
            int child = m_Storage.Count - 1;
            int parent = GetParentIndex( child );
            BookKeeper[ elem ] = BubbleUp( parent, child );
        }

        public override T Pop()
        {
            if ( m_Storage.Count == 0 )
                return default( T );
            T toRet = m_Storage[ 0 ];
            Swap( 0, m_Storage.Count - 1 );
            m_Storage.RemoveAt( m_Storage.Count - 1 );
            BubbleDown( 0 );
            BookKeeper.Remove( toRet );
            return toRet;
        }

        public bool Contains( T item )
        {
            return BookKeeper.ContainsKey( item );
        }
    }
}
