using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainWeightEntry
{
    public TerrainType terrain = TerrainType.Normal;
    [Range(0, 100)] public int weight = 50;
}

[CreateAssetMenu(fileName = "BiomeConfig", menuName = "SO/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    public BiomeType biomeType = BiomeType.Plains;
    public string displayName = "Plains";

    [Header("Terrain Generation")]
    public List<TerrainWeightEntry> terrainWeights = new List<TerrainWeightEntry>();
    [Range(0, 5)] public int baseHeight;
    [Range(0, 5)] public int maxHeightVariation = 1;

    [Header("Stat Modifiers")]
    [Range(0.5f, 2f)] public float habitatBonus = 1.2f;
    [Range(0.5f, 2f)] public float nonHabitatPenalty = 0.9f;
    public List<DamageTypeBiomeBonus> damageBonuses = new List<DamageTypeBiomeBonus>();

    [Header("Visual")]
    public Color gridTint = Color.white;
    public string groundVisualId = "plains";

    [Header("Spawn Pool")]
    public List<GachaUnitData> commonEnemies = new List<GachaUnitData>();
    public List<GachaUnitData> eliteEnemies = new List<GachaUnitData>();
    public GachaUnitData bossUnit;

    public TerrainType RollTerrain()
    {
        if (terrainWeights.Count == 0)
            return TerrainType.Normal;

        int total = 0;
        foreach (var t in terrainWeights) total += t.weight;
        if (total <= 0) return TerrainType.Normal;

        int roll = UnityEngine.Random.Range(0, total);
        foreach (var t in terrainWeights)
        {
            if (roll < t.weight) return t.terrain;
            roll -= t.weight;
        }
        return TerrainType.Normal;
    }

    public float GetDamageBonus(DamageType damageType)
    {
        foreach (var b in damageBonuses)
            if (b.damageType == damageType)
                return b.bonus;
        return 1f;
    }
}

[Serializable]
public class DamageTypeBiomeBonus
{
    public DamageType damageType = DamageType.Fire;
    [Range(0.5f, 2f)] public float bonus = 1f;
}
