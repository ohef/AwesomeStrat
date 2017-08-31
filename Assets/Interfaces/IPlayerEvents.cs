using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.EventSystems;

public interface IUnitDeathHandler : IEventSystemHandler
{
    void OnUnitDeath( Unit deadUnit );
}