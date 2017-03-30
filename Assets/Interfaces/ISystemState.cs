using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ISystemState
{
    void Update( BattleSystem state );
    void Enter( BattleSystem state );
    void Exit( BattleSystem state );
}