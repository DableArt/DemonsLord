using UnityEngine;

[CreateAssetMenu(fileName = "EvolutionRecipe", menuName = "SO/Evolution")]
public class EvolutionData : ScriptableObject
{
    public string recipeName = "Rank Up";
    public UnitRank fromRank = UnitRank.R;
    public UnitRank toRank = UnitRank.SR;
    public int requiredLevel = 10;
    public float statMultiplier = 1.3f;
    public int requiredKills;

    [Header("Visual")]
    public Color evolutionColor = Color.yellow;
    public string evolutionEffect = "RankUp";
}
