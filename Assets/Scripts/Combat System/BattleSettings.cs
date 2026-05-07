using UnityEngine;

public enum AIDifficulty { Easy, Normal, Hard }

[CreateAssetMenu(menuName = "Battle/BattleSettings")]
public class BattleSettings : ScriptableObject
{
    [Header("Difficulty")]
    public AIDifficulty difficulty = AIDifficulty.Normal;

    [Header("Combat")]
    [Tooltip("Делитель урона при защите")]
    public float defendDamageMultiplier = 2f;

    [Header("AI Behaviour")]
    [Tooltip("Отступать если HP < этого процента")]
    [Range(0f, 1f)] public float retreatHpPercent = 0.3f;
    [Tooltip("Шанс случайного хода на Easy")]
    [Range(0f, 1f)] public float randomMoveProbabilityEasy = 0.35f;
}
