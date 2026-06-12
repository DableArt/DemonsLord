using UnityEngine;

public class TestCombatStarter : MonoBehaviour
{
    public bool createDummyUnitsIfNoData = true;

    void Start()
    {
        var bm = GetComponent<BattleManager>();
        if (bm == null) return;

        bool playerHasUnits = false;
        bool enemyHasUnits = false;

        if (bm.gachaSystem == null)
            bm.gachaSystem = bm.gameObject.GetComponent<GachaSystem>();
        if (bm.gachaSystem == null)
            bm.gachaSystem = bm.gameObject.AddComponent<GachaSystem>();

        if (bm.gachaSystem.availableUnits.Count > 0)
        {
            bm.PopulatePlayerSquadFromGacha(3, 1);
            playerHasUnits = bm.playerSquad.AliveCount > 0;
        }

        bool hasBiomeWithEnemies = bm.battleBiome != null && bm.battleBiome.commonEnemies.Count > 0;
        if (hasBiomeWithEnemies)
        {
            bm.GenerateEnemySquadFromBiome(2);
            enemyHasUnits = bm.enemySquad.AliveCount > 0;
        }

        if (createDummyUnitsIfNoData && (!playerHasUnits || !enemyHasUnits))
        {
            Debug.LogWarning("TestCombatStarter: Создаю тестовых юнитов для обоих отрядов");

            if (!playerHasUnits)
            {
                for (int i = 0; i < 3; i++)
                {
                    var go = new GameObject($"DummyPlayer_{i}");
                    go.transform.position = new Vector3(100, 100, 0);
                    var unit = go.AddComponent<Unit>();
                    unit.Init($"Hero_{i + 1}", 1, 5, 4, 4, 5, 3, 3, 8, 5, 40, 15, UnitRank.SR, UnitHabitatType.Ground, UnitSize.Small);
                    unit.gameObject.AddComponent<SpellCaster>();
                    unit.gameObject.AddComponent<AbilityComponent>();
                    unit.gameObject.AddComponent<VfxTrigger>();
                    unit.gameObject.AddComponent<StatusManager>();
                    bm.playerSquad.AddUnit(unit);
                }
            }

            if (!enemyHasUnits)
            {
                for (int i = 0; i < 2; i++)
                {
                    var go = new GameObject($"DummyEnemy_{i}");
                    go.transform.position = new Vector3(100, 100, 0);
                    var unit = go.AddComponent<Unit>();
                    unit.Init($"Skeleton_{i + 1}", 1, 5, 3, 5, 2, 2, 2, 8, 5, 30, 10, UnitRank.R, UnitHabitatType.Ground, UnitSize.Small);
                    unit.gameObject.AddComponent<SpellCaster>();
                    unit.gameObject.AddComponent<AbilityComponent>();
                    unit.gameObject.AddComponent<VfxTrigger>();
                    unit.gameObject.AddComponent<StatusManager>();
                    bm.enemySquad.AddUnit(unit);
                }
            }
        }

        bm.StartBattle();
    }
}
