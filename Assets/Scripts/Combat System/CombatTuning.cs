using UnityEngine;

namespace DemonsLord.CombatSystem
{
    [CreateAssetMenu(fileName = "CombatTuning", menuName = "Combat/Combat Tuning")]
    public class CombatTuning : ScriptableObject
    {
        [Header("Critical")]
        [Range(0f, 1f)] public float CriticalChancePerLuck = 0.01f;
        [Min(1f)] public float CriticalDamageMultiplier = 1.5f;

        [Header("Defend")]
        [Min(1f)] public float DefendMultiplier = 2f;

        [Header("Escape")]
        [Min(0f)] public float PlayerEscapeBonus = 10f;
    }
}
