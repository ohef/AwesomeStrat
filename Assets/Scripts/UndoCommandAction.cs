using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct UndoCommandAction : IUndoCommand
{
    private Action execute;
    private Action undo;

    public UndoCommandAction( Action execute, Action undo )
    {
        this.execute = execute;
        this.undo = undo;
    }

    public void Execute()
    {
        execute();
    }

    public void Undo()
    {
        undo();
    }
}
