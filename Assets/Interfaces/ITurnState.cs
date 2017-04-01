using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ITurnState
{
    void Update( PlayerTurnController context );
    void Enter( PlayerTurnController context );
    void Exit( PlayerTurnController context );
}
