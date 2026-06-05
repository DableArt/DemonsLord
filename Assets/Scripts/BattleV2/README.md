# BattleV2 (milestone 1)

Новая изолированная боевая подсистема (без изменений старой `Combat System`).

## Что реализовано
- Вход в бой через `NpcBattleTrigger`
- Сохранение входных данных через `BattleEntryStorage`
- Загрузка отдельной боевой сцены
- Генерация поля `9x12` через `BattleGridGenerator`
- Спавн:
  - декоративного объекта игрока слева вне поля
  - боевого юнита игрока слева на поле
  - боевого юнита врага справа на поле
- Безопасные fallback-ы при отсутствии ссылок
- Точка расширения для визуала тайла мира: `IBattleTileVisualProvider`

## Быстрое подключение
1. Создайте новую сцену (например, `BattleSceneV2`) и добавьте её в Build Settings.
2. На пустой объект сцены добавьте:
   - `BattleGridGenerator`
   - `BattleSceneController`
   - `BattleSceneBootstrap`
3. В `BattleSceneController`:
   - Укажите ссылку на `BattleGridGenerator`
   - (Опционально) назначьте префабы `leftDecorativePlayerPrefab`, `playerUnitPrefab`, `enemyUnitPrefab`
4. На NPC в world-сцене добавьте `NpcBattleTrigger` и trigger-collider.
5. Назначьте `battleSceneName` равным имени боевой сцены.
6. Убедитесь, что у игрока проставлен тег `Player` (или настройте `playerTag`).

## Интеграция визуала тайла мира
Для передачи визуала клетки из мира реализуйте компонент с интерфейсом:
- `IBattleTileVisualProvider`

И назначьте его в `tileVisualProviderSource` у `NpcBattleTrigger`.
Если провайдер не задан/вернул `null`, будет использован fallback.
