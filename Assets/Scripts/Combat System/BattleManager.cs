using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Grid")]
    public GridManager gridManager;

    [Header("Biome")]
    public BiomeManager biomeManager;
    public BiomeConfig battleBiome;

    [Header("Gacha")]
    public GachaSystem gachaSystem;

    [Header("Squads")]
    public UnitSquad playerSquad = new UnitSquad { squadName = "Heroes" };
    public UnitSquad enemySquad = new UnitSquad { squadName = "Enemies" };

    [Header("Current State")]
    public BattlePhase currentPhase = BattlePhase.Preparation;
    public TurnManager turnManager = new TurnManager();
    public Unit selectedUnit;

    public bool isSelectingMagic;
    public bool isSelectingTarget;
    public SpellBase selectedSpell;

    private bool isProcessingTurn = false;

    void Start()
    {
        currentPhase = BattlePhase.Preparation;
        SetupBattle();
    }

    void Update()
    {
        if (currentPhase == BattlePhase.PlayerTurn && !isProcessingTurn)
        {
            HandleKeyboardInput();
            HandleMouseInput();
        }
    }

    void SetupBattle()
    {
        if (gridManager == null)
        {
            Debug.LogError("BattleManager: GridManager is not set.");
            return;
        }

        if (gridManager.grid == null)
            gridManager.ResetGrid();

        ApplyBiomeConfig();
        GrantDefaultSpells();
        GrantDefaultAbilities();

        DeploySquads();

        var allUnits = new List<Unit>();
        allUnits.AddRange(playerSquad.units.Where(u => u != null && u.IsAlive));
        allUnits.AddRange(enemySquad.units.Where(u => u != null && u.IsAlive));

        if (allUnits.Count == 0)
        {
            Debug.LogWarning("BattleManager: No units in any squad.");
            return;
        }

        turnManager.BuildOrder(allUnits);
        currentPhase = BattlePhase.Initiative;
        StartNextTurn();
    }

    void ApplyBiomeConfig()
    {
        if (biomeManager == null)
            biomeManager = GetComponent<BiomeManager>();
        if (biomeManager == null && gridManager != null)
        {
            biomeManager = gridManager.gameObject.AddComponent<BiomeManager>();
            biomeManager.gridManager = gridManager;
        }
        if (biomeManager != null && battleBiome != null)
        {
            biomeManager.ApplyBiomeConfig(battleBiome);
            Debug.Log($"[Battle] Biome applied: {battleBiome.displayName}");
        }
    }

    void GrantDefaultAbilities()
    {
        foreach (var unit in playerSquad.units)
        {
            if (unit == null) continue;
            var comp = unit.GetComponent<AbilityComponent>();
            if (comp != null) comp.Initialize();
        }
        foreach (var unit in enemySquad.units)
        {
            if (unit == null) continue;
            var comp = unit.GetComponent<AbilityComponent>();
            if (comp != null) comp.Initialize();
        }
    }

    void GrantDefaultSpells()
    {
        foreach (var unit in playerSquad.units)
        {
            if (unit == null) continue;
            var caster = unit.GetComponent<SpellCaster>();
            if (caster != null && caster.knownSpells.Count == 0)
            {
                caster.knownSpells.Add(DefaultSpells.Fireball);
                caster.knownSpells.Add(DefaultSpells.IceBolt);
                caster.knownSpells.Add(DefaultSpells.LightHeal);
            }
        }
        foreach (var unit in enemySquad.units)
        {
            if (unit == null) continue;
            var caster = unit.GetComponent<SpellCaster>();
            if (caster != null && caster.knownSpells.Count == 0)
            {
                caster.knownSpells.Add(DefaultSpells.DarkBolt);
            }
        }
    }

    public void PopulatePlayerSquadFromGacha(int count, int level = 1)
    {
        if (gachaSystem == null)
        {
            gachaSystem = GetComponent<GachaSystem>();
            if (gachaSystem == null)
                gachaSystem = gameObject.AddComponent<GachaSystem>();
        }
        for (int i = 0; i < count; i++)
        {
            var unit = gachaSystem.PullUnit(level);
            if (unit != null)
                playerSquad.AddUnit(unit);
        }
    }

    public void GenerateEnemySquadFromBiome(int count)
    {
        if (battleBiome == null) return;
        var pool = battleBiome.commonEnemies;
        if (pool.Count == 0)
        {
            Debug.LogWarning("Biome has no common enemies defined.");
            return;
        }
        for (int i = 0; i < count; i++)
        {
            var data = pool[Random.Range(0, pool.Count)];
            int level = Mathf.Max(1, playerSquad.AliveCount > 0
                ? playerSquad.GetAliveUnitBySlot(0).unitLevel : 1);
            var unit = GachaSystem.CreateUnitFromData(data, data.RollRank(), level);
            if (unit != null)
                enemySquad.AddUnit(unit);
        }
        if (battleBiome.bossUnit != null)
        {
            var bossData = battleBiome.bossUnit;
            int bossLevel = Mathf.Max(1, playerSquad.AliveCount > 0
                ? playerSquad.GetAliveUnitBySlot(0).unitLevel + 2 : 3);
            var boss = GachaSystem.CreateUnitFromData(bossData, UnitRank.SSR, bossLevel);
            if (boss != null)
            {
                boss.unitName = $"[BOSS] {boss.unitName}";
                var bossAI = boss.gameObject.AddComponent<BossAI>();
                bossAI.bossTitle = boss.unitName;
                enemySquad.AddUnit(boss);
            }
        }
    }

    void DeploySquads()
    {
        int midY = gridManager.grid.height / 2;
        int playerX = 1;
        int enemyX = gridManager.grid.width - 2;

        for (int i = 0; i < playerSquad.units.Count; i++)
        {
            var unit = playerSquad.units[i];
            if (unit == null) continue;
            Vector2Int pos = new Vector2Int(playerX, midY + i - playerSquad.units.Count / 2);
            if (!gridManager.grid.IsWithinBounds(pos)) continue;
            if (gridManager.CanOccupy(unit, pos))
                gridManager.PlaceUnit(unit, pos);
        }

        for (int i = 0; i < enemySquad.units.Count; i++)
        {
            var unit = enemySquad.units[i];
            if (unit == null) continue;
            Vector2Int pos = new Vector2Int(enemyX, midY + i - enemySquad.units.Count / 2);
            if (!gridManager.grid.IsWithinBounds(pos)) continue;
            if (gridManager.CanOccupy(unit, pos))
                gridManager.PlaceUnit(unit, pos);
        }
    }

    void StartNextTurn()
    {
        isProcessingTurn = false;
        isSelectingMagic = false;
        isSelectingTarget = false;
        selectedSpell = null;

        if (!playerSquad.IsAlive)
        {
            currentPhase = BattlePhase.Lost;
            OnBattleEnd(false);
            return;
        }
        if (!enemySquad.IsAlive)
        {
            currentPhase = BattlePhase.Won;
            OnBattleEnd(true);
            return;
        }

        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive)
        {
            turnManager.NextTurn();
            StartNextTurn();
            return;
        }

        ClearUnitSelection();

        var abilityComp = current?.GetComponent<AbilityComponent>();
        abilityComp?.OnTurnStart();

        if (playerSquad.units.Contains(current))
        {
            currentPhase = BattlePhase.PlayerTurn;
            SelectUnit(current);
            OnPlayerTurnStart();
        }
        else
        {
            currentPhase = BattlePhase.EnemyTurn;
            StartCoroutine(ExecuteEnemyTurn(current));
        }
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var next = playerSquad.GetNextAliveUnit(selectedUnit);
            if (next != null)
            {
                CancelMagicMode();
                SelectUnit(next);
            }
        }

        for (int i = 0; i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                if (isSelectingMagic)
                {
                    SelectSpellBySlot(i);
                    return;
                }
                var unit = playerSquad.GetAliveUnitBySlot(i);
                if (unit != null)
                {
                    CancelMagicMode();
                    SelectUnit(unit);
                }
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            OnPlayerUltimate();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isSelectingTarget)
        {
            CancelMagicMode();
        }
    }

    void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (isSelectingTarget && selectedSpell != null)
        {
            TryCastSpellAtMouse();
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = new Vector2Int(
            Mathf.RoundToInt(mouseWorld.x),
            Mathf.RoundToInt(mouseWorld.y)
        );

        if (!gridManager.grid.IsWithinBounds(cell)) return;

        Unit clickedUnit = FindUnitAtCell(cell);
        if (clickedUnit != null)
        {
            if (playerSquad.units.Contains(clickedUnit) && clickedUnit.IsAlive)
            {
                CancelMagicMode();
                SelectUnit(clickedUnit);
                return;
            }
            return;
        }

        var current = turnManager.CurrentUnit;
        if (selectedUnit == current && current != null && current.IsAlive)
        {
            if (gridManager.grid.IsOccupied(cell))
            {
                var target = enemySquad.GetUnitAtPosition(cell);
                if (target != null && IsAdjacent(current.gridPosition, cell))
                {
                    ExecuteAttack(current, target);
                    return;
                }
                return;
            }

            var path = PathFindingHelper.FindPath(gridManager.grid, current.gridPosition, cell, current);
            if (path.IsValid && path.Length > 1)
            {
                gridManager.MoveUnit(current, path.End);
                ClearUnitSelection();
                EndPlayerTurn();
            }
        }
    }

    void SelectSpellBySlot(int slotIndex)
    {
        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive) return;

        var caster = current.GetComponent<SpellCaster>();
        if (caster == null || slotIndex >= caster.knownSpells.Count) return;

        var spell = caster.knownSpells[slotIndex];
        if (caster.CanCast(spell))
        {
            selectedSpell = spell;
            isSelectingTarget = true;
            isSelectingMagic = false;
            OnTurnUIUpdate();
        }
    }

    void TryCastSpellAtMouse()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = new Vector2Int(
            Mathf.RoundToInt(mouseWorld.x),
            Mathf.RoundToInt(mouseWorld.y)
        );

        if (!gridManager.grid.IsWithinBounds(cell)) return;

        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive) return;

        var caster = current.GetComponent<SpellCaster>();
        if (caster == null) return;

        int dist = Mathf.Abs(current.gridPosition.x - cell.x) + Mathf.Abs(current.gridPosition.y - cell.y);
        if (dist > selectedSpell.range) return;

        CancelMagicMode();
        StartCoroutine(ExecuteSpellCast(current, caster, selectedSpell, cell));
    }

    IEnumerator ExecuteSpellCast(Unit casterUnit, SpellCaster caster, SpellBase spell, Vector2Int targetCell)
    {
        isProcessingTurn = true;

            yield return caster.CastSpell(spell, targetCell, gridManager.grid,
                enemySquad, playerSquad, (totalDamage) =>
                {
                    foreach (var unit in enemySquad.units)
                    {
                        if (unit != null && !unit.IsAlive && turnManager.CurrentUnit != unit)
                        {
                            int expGain = 10 + unit.unitLevel * 5 + (int)unit.rank * 5;
                            casterUnit.currentExp += expGain;
                            CheckLevelUp(casterUnit);

                            var deathVfx = unit.GetComponent<VfxTrigger>();
                            deathVfx?.TriggerDeath();
                            turnManager.RemoveUnit(unit);
                        }
                    }
                    foreach (var unit in playerSquad.units)
                    {
                        if (unit != null && !unit.IsAlive && turnManager.CurrentUnit != unit)
                        {
                            var deathVfx = unit.GetComponent<VfxTrigger>();
                            deathVfx?.TriggerDeath();
                            turnManager.RemoveUnit(unit);
                        }
                    }
                });

        ClearUnitSelection();
        yield return new WaitForSeconds(0.5f);
        EndPlayerTurn();
    }

    Unit FindUnitAtCell(Vector2Int cell)
    {
        var unit = playerSquad.GetUnitAtPosition(cell);
        if (unit != null) return unit;
        return enemySquad.GetUnitAtPosition(cell);
    }

    void SelectUnit(Unit unit)
    {
        ClearUnitSelection();
        selectedUnit = unit;
        if (unit != null) unit.IsSelected = true;
        OnTurnUIUpdate();
    }

    void ClearUnitSelection()
    {
        if (selectedUnit != null)
            selectedUnit.IsSelected = false;
        selectedUnit = null;
    }

    void CancelMagicMode()
    {
        isSelectingMagic = false;
        isSelectingTarget = false;
        selectedSpell = null;
        OnTurnUIUpdate();
    }

    void ExecuteAttack(Unit attacker, Unit target)
    {
        isProcessingTurn = true;

        var posMod = DamageCalculator.GetPositionModifier(
            attacker.gridPosition, target.gridPosition, gridManager.grid);
        bool isCrit = DamageCalculator.IsCriticalHit(attacker);
        int damage = DamageCalculator.CalculatePhysicalDamage(
            attacker, target, posMod, RangeType.Melee, isCrit);
        damage = ApplyBiomeDamage(attacker, damage);

        target.TakeDamage(damage);

        attacker.GainUltimateCharge(damage / 2);
        target.GainUltimateCharge(damage / 4);

        var vfx = attacker.GetComponent<VfxTrigger>();
        vfx?.TriggerDamageDealt(damage);
        var targetVfx = target.GetComponent<VfxTrigger>();
        targetVfx?.TriggerDamageTaken(damage);

        FloatingDamage.ShowDamage(target, damage, isCrit);

        if (isCrit)
            Debug.Log($"CRIT! {attacker.unitName} deals {damage} damage to {target.unitName}");
        else
            Debug.Log($"{attacker.unitName} deals {damage} damage to {target.unitName}");

        if (!target.IsAlive)
        {
            int expGain = 10 + target.unitLevel * 5 + (int)target.rank * 5;
            attacker.currentExp += expGain;
            CheckLevelUp(attacker);

            var deathVfx = target.GetComponent<VfxTrigger>();
            deathVfx?.TriggerDeath();
            gridManager.RemoveUnitFromGrid(target);
            turnManager.RemoveUnit(target);
        }

        ClearUnitSelection();
        EndPlayerTurn();
    }

    void EndPlayerTurn()
    {
        isSelectingMagic = false;
        isSelectingTarget = false;
        selectedSpell = null;

        currentPhase = BattlePhase.Reaction;
        turnManager.NextTurn();
        currentPhase = BattlePhase.RoundEnd;
        StartNextTurn();
    }

    IEnumerator ExecuteEnemyTurn(Unit enemyUnit)
    {
        isProcessingTurn = true;
        yield return new WaitForSeconds(0.5f);

        if (!playerSquad.IsAlive)
        {
            StartNextTurn();
            yield break;
        }

        var enemyAI = enemyUnit.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            yield return enemyAI.ExecuteTurn(this);
        }
        else
        {
            yield return ExecuteFallbackAI(enemyUnit);
        }

        yield return new WaitForSeconds(0.5f);
        isProcessingTurn = false;
        currentPhase = BattlePhase.Reaction;
        turnManager.NextTurn();
        currentPhase = BattlePhase.RoundEnd;
        StartNextTurn();
    }

    IEnumerator ExecuteFallbackAI(Unit enemyUnit)
    {
        bool acted = false;

        var enemyCaster = enemyUnit.GetComponent<SpellCaster>();
        if (enemyCaster != null && enemyCaster.knownSpells.Count > 0)
        {
            var nearest = FindNearestPlayerUnit(enemyUnit.gridPosition);
            if (nearest != null)
            {
                int dist = Mathf.Abs(enemyUnit.gridPosition.x - nearest.gridPosition.x)
                         + Mathf.Abs(enemyUnit.gridPosition.y - nearest.gridPosition.y);

                if (dist <= 4)
                {
                    var spell = enemyCaster.knownSpells[0];
                    if (enemyCaster.CanCast(spell))
                    {
                        yield return enemyCaster.CastSpell(spell, nearest.gridPosition,
                            gridManager.grid, playerSquad, enemySquad, (dmg) =>
                            {
                                if (!nearest.IsAlive)
                                {
                                    var deathVfx = nearest.GetComponent<VfxTrigger>();
                                    deathVfx?.TriggerDeath();
                                    gridManager.RemoveUnitFromGrid(nearest);
                                    turnManager.RemoveUnit(nearest);
                                }
                            });
                        acted = true;
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
        }

        if (!acted)
        {
            var nearestPlayer = FindNearestPlayerUnit(enemyUnit.gridPosition);
            if (nearestPlayer != null)
            {
                if (IsAdjacent(enemyUnit.gridPosition, nearestPlayer.gridPosition))
                {
                    var posMod = DamageCalculator.GetPositionModifier(
                        enemyUnit.gridPosition, nearestPlayer.gridPosition, gridManager.grid);
                    bool isCrit = DamageCalculator.IsCriticalHit(enemyUnit);
                    int damage = DamageCalculator.CalculatePhysicalDamage(
                        enemyUnit, nearestPlayer, posMod, RangeType.Melee, isCrit);
                    damage = ApplyBiomeDamage(enemyUnit, damage);

                    nearestPlayer.TakeDamage(damage);

                    enemyUnit.GainUltimateCharge(damage / 2);
                    nearestPlayer.GainUltimateCharge(damage / 4);

                    var vfx = enemyUnit.GetComponent<VfxTrigger>();
                    vfx?.TriggerDamageDealt(damage);
                    var playerVfx = nearestPlayer.GetComponent<VfxTrigger>();
                    playerVfx?.TriggerDamageTaken(damage);

                    FloatingDamage.ShowDamage(nearestPlayer, damage, isCrit);

                    if (isCrit)
                        Debug.Log($"CRIT! {enemyUnit.unitName} deals {damage} damage to {nearestPlayer.unitName}");

                    if (!nearestPlayer.IsAlive)
                    {
                        int expGain = 10 + nearestPlayer.unitLevel * 5 + (int)nearestPlayer.rank * 5;
                        enemyUnit.currentExp += expGain;
                        CheckLevelUp(enemyUnit);

                        var deathVfx = nearestPlayer.GetComponent<VfxTrigger>();
                        deathVfx?.TriggerDeath();
                        gridManager.RemoveUnitFromGrid(nearestPlayer);
                        turnManager.RemoveUnit(nearestPlayer);
                    }
                }
                else
                {
                    var path = PathFindingHelper.FindPath(
                        gridManager.grid,
                        enemyUnit.gridPosition,
                        nearestPlayer.gridPosition,
                        enemyUnit
                    );
                    if (path.IsValid && path.Length > 1)
                    {
                        Vector2Int nextCell = path[1];
                        if (gridManager.grid.IsWithinBounds(nextCell) && !gridManager.grid.IsOccupied(nextCell))
                            gridManager.MoveUnit(enemyUnit, nextCell);
                    }
                }
            }
        }
    }

    Unit FindNearestPlayerUnit(Vector2Int from)
    {
        Unit nearest = null;
        int minDist = int.MaxValue;
        foreach (var unit in playerSquad.units)
        {
            if (unit == null || !unit.IsAlive) continue;
            int dist = Mathf.Abs(unit.gridPosition.x - from.x) + Mathf.Abs(unit.gridPosition.y - from.y);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = unit;
            }
        }
        return nearest;
    }

    public void OnPlayerAttack()
    {
        if (currentPhase != BattlePhase.PlayerTurn || isProcessingTurn) return;
        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive) return;

        var adjacentEnemy = FindAdjacentEnemy(current);
        if (adjacentEnemy != null)
            ExecuteAttack(current, adjacentEnemy);
    }

    public void OnPlayerDefend()
    {
        if (currentPhase != BattlePhase.PlayerTurn || isProcessingTurn) return;
        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive) return;
        CancelMagicMode();
        ClearUnitSelection();
        EndPlayerTurn();
    }

    public void OnPlayerWait()
    {
        if (currentPhase != BattlePhase.PlayerTurn || isProcessingTurn) return;
        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive) return;
        CancelMagicMode();
        ClearUnitSelection();
        EndPlayerTurn();
    }

    public void OnPlayerSelectMagic()
    {
        if (currentPhase != BattlePhase.PlayerTurn || isProcessingTurn) return;
        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive) return;

        var caster = current.GetComponent<SpellCaster>();
        if (caster == null || caster.knownSpells.Count == 0) return;

        isSelectingMagic = true;
        isSelectingTarget = false;
        selectedSpell = null;
        OnTurnUIUpdate();
    }

    Unit FindAdjacentEnemy(Unit unit)
    {
        if (unit == null) return null;
        foreach (var enemy in enemySquad.units)
        {
            if (enemy != null && enemy.IsAlive && IsAdjacent(unit.gridPosition, enemy.gridPosition))
                return enemy;
        }
        return null;
    }

    void OnPlayerTurnStart()
    {
        OnTurnUIUpdate();
    }

    public void OnPlayerUltimate()
    {
        if (currentPhase != BattlePhase.PlayerTurn || isProcessingTurn) return;
        var current = turnManager.CurrentUnit;
        if (current == null || !current.IsAlive) return;

        var abilityComp = current.GetComponent<AbilityComponent>();
        if (abilityComp == null || !abilityComp.CanUseUltimate()) return;

        isProcessingTurn = true;

        var nearestEnemy = FindNearestEnemyUnit(current.gridPosition);
        if (nearestEnemy != null)
        {
            int damage = abilityComp.ultimateAbility.power + Mathf.RoundToInt(current.intelligence * 0.5f);
            nearestEnemy.TakeDamage(damage);

            current.GainUltimateCharge(0);
            abilityComp.UseUltimate();

            var vfx = current.GetComponent<VfxTrigger>();
            vfx?.TriggerDamageDealt(damage);
            vfx?.TriggerUltimate();
            var targetVfx = nearestEnemy.GetComponent<VfxTrigger>();
            targetVfx?.TriggerDamageTaken(damage);

            FloatingDamage.ShowUltimate(nearestEnemy, damage);

            Debug.Log($"[ULTIMATE] {current.unitName} использует {abilityComp.ultimateAbility.abilityName} на {nearestEnemy.unitName} - {damage} урона!");

            if (!nearestEnemy.IsAlive)
            {
                gridManager.RemoveUnitFromGrid(nearestEnemy);
                turnManager.RemoveUnit(nearestEnemy);
                var deathVfx = nearestEnemy.GetComponent<VfxTrigger>();
                deathVfx?.TriggerDeath();
            }
        }
        else
        {
            var nearestAlly = FindLowestHpAlly(current.gridPosition);
            if (nearestAlly != null)
            {
                int heal = abilityComp.ultimateAbility.power + Mathf.RoundToInt(current.intelligence * 0.5f);
                nearestAlly.Heal(heal);

                abilityComp.UseUltimate();

                var targetVfx = nearestAlly.GetComponent<VfxTrigger>();
                targetVfx?.TriggerHealReceived(heal);
                targetVfx?.TriggerUltimate();

                FloatingDamage.ShowHeal(nearestAlly, heal);

                Debug.Log($"[ULTIMATE] {current.unitName} использует {abilityComp.ultimateAbility.abilityName} на {nearestAlly.unitName} - {heal} лечения!");
            }
        }

        ClearUnitSelection();
        EndPlayerTurn();
    }

    void CheckLevelUp(Unit unit)
    {
        if (unit == null) return;
        while (unit.currentExp >= unit.expToNextLevel)
        {
            unit.currentExp -= unit.expToNextLevel;
            UnitEvolution.LevelUp(unit, 1);
            unit.expToNextLevel = Mathf.RoundToInt(unit.expToNextLevel * 1.2f);
        }
    }

    public static int ApplyBiomeDamage(Unit attacker, int baseDamage)
    {
        float mod = BiomeManager.GetDamageModifier(attacker);
        return Mathf.RoundToInt(baseDamage * mod);
    }

    Unit FindNearestEnemyUnit(Vector2Int from)
    {
        Unit nearest = null;
        int minDist = int.MaxValue;
        foreach (var unit in enemySquad.units)
        {
            if (unit == null || !unit.IsAlive) continue;
            int dist = Mathf.Abs(unit.gridPosition.x - from.x) + Mathf.Abs(unit.gridPosition.y - from.y);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = unit;
            }
        }
        return nearest;
    }

    Unit FindLowestHpAlly(Vector2Int from)
    {
        Unit lowest = null;
        int minHp = int.MaxValue;
        foreach (var unit in playerSquad.units)
        {
            if (unit == null || !unit.IsAlive) continue;
            if (unit.currentHP < minHp)
            {
                minHp = unit.currentHP;
                lowest = unit;
            }
        }
        return lowest;
    }

    void OnTurnUIUpdate()
    {
        var ui = FindObjectOfType<UIManager>();
        if (ui == null) return;

        var current = turnManager.CurrentUnit;
        var display = selectedUnit ?? current;

        if (display != null) ui.UpdateUnitUI(display);
        ui.UpdateTurnOrderDisplay();

        ui.UpdateTopPanel(
            turnManager.TurnNumber,
            currentPhase.ToString(),
            current != null ? current.unitName : "None"
        );

        ui.UpdateBattleStats(
            playerSquad.AliveCount, playerSquad.units.Count,
            enemySquad.AliveCount, enemySquad.units.Count
        );

        ui.UpdateSquadList(playerSquad, selectedUnit);

        if (isSelectingMagic)
        {
            var caster = current?.GetComponent<SpellCaster>();
            ui.ShowMagicPanel(caster);
        }
        else if (isSelectingTarget && selectedSpell != null)
        {
            ui.ShowTargetingInfo(selectedSpell);
        }
        else
        {
            ui.HideMagicPanel();
        }
    }

    void OnBattleEnd(bool playerWon)
    {
        ClearUnitSelection();
        CancelMagicMode();
        if (playerWon)
            Debug.Log("Игрок победил!");
        else
            Debug.Log("Игрок проиграл!");
        OnTurnUIUpdate();
    }

    bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }
}
