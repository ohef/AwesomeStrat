using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ITurnState
{
    void UpdateState( PlayerTurnController context );
    void EnterState( PlayerTurnController context );
    void ExitState( PlayerTurnController context );
}
