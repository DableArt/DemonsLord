using UnityEngine;

namespace DemonsLord.CombatSystem
{
    [CreateAssetMenu(fileName = "UnitStatsDefinition", menuName = "Combat/Unit Stats Definition")]
    public class UnitStatsDefinition : ScriptableObject
    {
        [Min(0)] public int Attack = 3;
        [Min(0)] public int Ag = 1;
        [Min(0)] public int Luc = 5;
        [Min(0)] public int Int = 1;
        [Min(0)] public int Def = 1;
    }
}
