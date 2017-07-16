using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.General.DataStructures
{
    public interface IContains<T>
    {
        bool Contains( T item );
    }

    public interface IHeap<T>
    {
        void Push( T elem );
        T Pop();
        int Count { get; }
    }

    public interface IState
    {
        void Enter();
        void Exit();
    }
}
