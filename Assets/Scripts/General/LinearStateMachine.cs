using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.General
{
    /// <summary>
    /// TODO: This doesn't communicate that it creates its own List and is not an actual iterator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReversableIterator<T> where T : class
    {
        protected LinkedList<T> Entries;
        protected LinkedListNode<T> current;
        public T Current { get { return current.Value; } }

        public ReversableIterator( IEnumerable<T> entries )
        {
            Entries = new LinkedList<T>( entries );
            current = Entries.First;
        }

        public virtual T Next()
        {
            if ( current.Next == null )
            {
                current = OnNextNull();
                return current.Value;
            }
            else
            {
                current = current.Next;
                return current.Value;
            }
        }

        public virtual T Previous()
        {
            if ( current.Previous == null )
            {
                current = OnPreviousNull();
                return current.Value;
            }
            else
            {
                current = current.Previous;
                return current.Value;
            }
        }

        protected virtual LinkedListNode<T> OnPreviousNull()
        {
            return null;
        }

        protected virtual LinkedListNode<T> OnNextNull()
        {
            return null;
        }
    }

    public class ReveresableCyclicIterator<T> : ReversableIterator<T> where T : class
    {
        public ReveresableCyclicIterator( IEnumerable<T> entries ) : base( entries )
        {
        }

        protected override LinkedListNode<T> OnPreviousNull()
        {
            return Entries.Last;
        }

        protected override LinkedListNode<T> OnNextNull()
        {
            return Entries.First;
        }
    }

    public class LinearStateMachine<T> : ReveresableCyclicIterator<T> where T : class, IState
    {
        public LinearStateMachine( IEnumerable<T> entries ) : base( entries )
        {
            Current.Enter();
        }

        public override T Next()
        {
            Current.Exit();
            base.Next();
            Current.Enter();
            return Current;
        }

        public override T Previous()
        {
            Current.Exit();
            base.Previous();
            Current.Enter();
            return Current;
        }
    }
}