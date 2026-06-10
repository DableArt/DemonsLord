using System.Collections.Generic;
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    public static BiomeManager Instance { get; private set; }

    public BiomeConfig currentConfig;
    public GridManager gridManager;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static float GetDamageModifier(Unit unit)
    {
        if (Instance == null || Instance.currentConfig == null || unit == null) return 1f;
        return Instance.GetUnitStatModifier(unit);
    }

    public static float GetDamageTypeModifierStatic(DamageType damageType)
    {
        if (Instance == null || Instance.currentConfig == null) return 1f;
        return Instance.GetDamageTypeModifier(damageType);
    }

    public void ApplyBiomeConfig(BiomeConfig config)
    {
        if (config == null || gridManager == null || gridManager.grid == null) return;
        currentConfig = config;

        var grid = gridManager.grid;
        int w = grid.width;
        int h = grid.height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var pos = new Vector2Int(x, y);
                var cell = grid.GetCell(pos);
                if (cell == null) continue;

                if (!cell.occupied)
                {
                    cell.terrain = config.RollTerrain();
                    cell.height = config.baseHeight + Random.Range(0, config.maxHeightVariation + 1);
                }
            }
        }

        SyncBiomeToGridManager(config);
        Debug.Log($"[Biome] Applied: {config.displayName} on {w}x{h} grid");
    }

    public void GenerateGridWithBiome(BiomeConfig config, int width, int height)
    {
        if (config == null || gridManager == null) return;
        currentConfig = config;

        gridManager.width = width;
        gridManager.height = height;

        var cellDataList = new List<GridManager.GridCellData>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cellDataList.Add(new GridManager.GridCellData
                {
                    point = new Vector2Int(x, y),
                    terrain = config.RollTerrain(),
                    height = config.baseHeight + Random.Range(0, config.maxHeightVariation + 1),
                    occupied = false
                });
            }
        }
        gridManager.cellData = cellDataList.ToArray();
        gridManager.biome = config.biomeType;
        gridManager.ResetGrid();

        Debug.Log($"[Biome] Generated {width}x{height} grid with {config.displayName}");
    }

    public float GetUnitStatModifier(Unit unit)
    {
        if (currentConfig == null || unit == null) return 1f;
        if (unit.habitatType == GetHabitatForBiome(currentConfig.biomeType))
            return currentConfig.habitatBonus;
        return currentConfig.nonHabitatPenalty;
    }

    public float GetDamageTypeModifier(DamageType damageType)
    {
        if (currentConfig == null) return 1f;
        return currentConfig.GetDamageBonus(damageType);
    }

    public static UnitHabitatType GetHabitatForBiome(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.DarkForest: return UnitHabitatType.Ground;
            case BiomeType.CursedSwamp: return UnitHabitatType.Water;
            case BiomeType.FireLands: return UnitHabitatType.Ground;
            case BiomeType.IceWastes: return UnitHabitatType.Ground;
            case BiomeType.BloodPlains: return UnitHabitatType.Ground;
            case BiomeType.EnchantedGrove: return UnitHabitatType.Ethereal;
            case BiomeType.ShadowDesert: return UnitHabitatType.Ground;
            case BiomeType.MountainFortress: return UnitHabitatType.Ground;
            case BiomeType.DeadLands: return UnitHabitatType.Underground;
            case BiomeType.EvilLands: return UnitHabitatType.Ethereal;
            case BiomeType.Hell: return UnitHabitatType.Ethereal;
            case BiomeType.Plains: return UnitHabitatType.Ground;
            case BiomeType.Desert: return UnitHabitatType.Ground;
            default: return UnitHabitatType.Ground;
        }
    }

    private void SyncBiomeToGridManager(BiomeConfig config)
    {
        if (gridManager != null)
            gridManager.biome = config.biomeType;
    }
}
