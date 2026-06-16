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

            var circleSprite = CreateCircleSprite();

            if (!playerHasUnits)
            {
                for (int i = 0; i < 3; i++)
                {
                    var go = new GameObject($"DummyPlayer_{i}");
                    go.transform.position = new Vector3(100, 100, 0);
                    var unit = go.AddComponent<Unit>();
                    unit.Init($"Hero_{i + 1}", 1, 8, 4, 4, 5, 3, 3, 8, 5, 40, 15, UnitRank.SR, UnitHabitatType.Ground, UnitSize.Small);
                    unit.gameObject.AddComponent<SpellCaster>();
                    unit.gameObject.AddComponent<AbilityComponent>();
                    unit.gameObject.AddComponent<VfxTrigger>();
                    unit.gameObject.AddComponent<StatusManager>();
                    AddUnitVisual(unit, circleSprite, Color.green);
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
                    unit.Init($"Skeleton_{i + 1}", 1, 8, 3, 5, 2, 2, 2, 8, 5, 30, 10, UnitRank.R, UnitHabitatType.Ground, UnitSize.Small);
                    unit.gameObject.AddComponent<SpellCaster>();
                    unit.gameObject.AddComponent<AbilityComponent>();
                    unit.gameObject.AddComponent<VfxTrigger>();
                    unit.gameObject.AddComponent<StatusManager>();
                    unit.gameObject.AddComponent<EnemyAI>();
                    AddUnitVisual(unit, circleSprite, Color.red);
                    bm.enemySquad.AddUnit(unit);
                }
            }
        }

        bm.StartBattle();
    }

    void AddUnitVisual(Unit unit, Sprite sprite, Color color)
    {
        var sr = unit.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = unit.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 10;
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        var tex = new Texture2D(size, size);
        var colors = new Color[size * size];
        float cx = size / 2f, cy = size / 2f, r = size / 2f - 1;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                colors[y * size + x] = (dx * dx + dy * dy <= r * r) ? Color.white : Color.clear;
            }
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
