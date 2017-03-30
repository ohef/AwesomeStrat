using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IUndoCommand : ICommand
{
    void Undo();
}