using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ITurnState
{
    void Update( TurnState context );
    void Enter( TurnState context );
    void Exit( TurnState context );
}
