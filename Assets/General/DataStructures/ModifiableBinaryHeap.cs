using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Assets.General.DataStructures
{
    public abstract class HeapItem
    {
        protected delegate void ValueChangedHandler();
        protected event ValueChangedHandler ValueChanged;

        public int HeapIndex { get; set; }
    }

    public class ModifiableBinaryHeap<T> : BinaryHeap<T>, IContains<T> where T : HeapItem, IComparable<T>
    {
        public void ValueChanged( T item )
        {
            if ( item.HeapIndex > 0 || item.HeapIndex < m_Storage.Count )
            {
                item.HeapIndex = BubbleUp( GetParentIndex( item.HeapIndex ), item.HeapIndex );
                item.HeapIndex = BubbleDown( item.HeapIndex );
            }
        }

        protected override void Swap( int aIndex, int bIndex )
        {
            int saved = m_Storage[ aIndex ].HeapIndex;
            m_Storage[ aIndex ].HeapIndex = bIndex;
            m_Storage[ bIndex ].HeapIndex = saved;
            base.Swap( aIndex, bIndex );
        }

        public override void Push( T elem )
        {
            m_Storage.Add( elem );
            int child = m_Storage.Count - 1;
            int parent = GetParentIndex( child );
            elem.HeapIndex = BubbleUp( parent, child );
        }

        public bool Contains( T item )
        {
            if ( item.HeapIndex > 0 || item.HeapIndex < m_Storage.Count )
                return m_Storage[ item.HeapIndex ].CompareTo( item ) == 0;
            return false;
        }
    }
}
