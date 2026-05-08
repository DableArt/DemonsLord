using UnityEngine;

namespace DemonsLord.CombatSystem
{
    [CreateAssetMenu(fileName = "UnitStatsDefinition", menuName = "Combat/Unit Stats Definition")]
    public class UnitStatsDefinition : ScriptableObject
    {
        [Min(0)] public int Attack = 3;
        [Tooltip("Agility")]
        [Min(0)] public int Ag = 1;
        [Tooltip("Luck")]
        [Min(0)] public int Luc = 5;
        [Tooltip("Intelligence")]
        [Min(0)] public int Int = 1;
        [Tooltip("Defense")]
        [Min(0)] public int Def = 1;
    }
}
