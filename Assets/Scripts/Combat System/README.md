# Combat System MVP

Код боевой системы находится в `/home/runner/work/DemonsLord/DemonsLord/Assets/Scripts/Combat System`.

## Что есть в MVP

- `CombatService` — entrypoint для старта боя.
- `BattleSession` — runtime боя 1v1 на фиксированной доске `12x8`.
- `BattleBoard` — поле и проверка перемещений/соседства.
- `UnitRuntime` — runtime-юнит со ссылками на low/high stats.
- `UnitStatsDefinition` — `ScriptableObject` для low stats (`Attack`, `Ag`, `Luc`, `Int`, `Def`).
- `UnitHighStatsData` — сериализуемые high stats (`HP`, `Mana`, `Ult`, `Level`) для JSON.
- `CombatAutoSave` — autosave в `Application.persistentDataPath/combat_autosave.json`.
- `BattleBackgroundDefinition` / `BattleBackgroundCatalog` / `BattleBackgroundView` — раздельные данные и view для biome background.

## Старт боя из кода

```csharp
using DemonsLord.CombatSystem;

var combatService = new CombatService();

var context = new BattleWorldContext
{
    TileId = "grass",
    BiomeId = "plains",
    Player = new BattleParticipantSetup
    {
        UnitId = "player",
        DisplayName = "Player",
        StatsDefinition = playerStatsDefinition,
        HighStats = playerHighStats
    },
    Npc = new BattleParticipantSetup
    {
        UnitId = "npc",
        DisplayName = "NPC",
        StatsDefinition = npcStatsDefinition,
        HighStats = npcHighStats
    }
};

BattleSession session = combatService.StartBattle(context, combatTuning);
```

## Reactive API

UI может подписываться на:

- `session.State`
- `session.CurrentSide`
- `session.CurrentUnit`
- `session.SelectedUnit`
- `session.MoveHighlights`
- `session.AttackHighlights`
- `session.UnitMoved`
- `session.UnitDamaged`
- `session.UnitDied`

## Пример без UI

Добавь `SampleBattleStarter` на любой `GameObject`, назначь:

1. `CombatTuning`
2. `UnitStatsDefinition` для Player и NPC
3. high stats в инспекторе

После этого можно вызвать `Start Sample Battle` через context menu компонента.
