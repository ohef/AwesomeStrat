using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu]
public class UnitClass : ScriptableObject
{
    public string Name;
    public List<Ability> ClassAbilities;

    public IEnumerable<Ability> GetActiveAbilities( Unit unit )
    {
        yield return WaitAbility.Instance;
        foreach ( var ability in ClassAbilities.Where( ability => ability.Useable( unit ) ) )
        {
            yield return ability;
        }
    }
}