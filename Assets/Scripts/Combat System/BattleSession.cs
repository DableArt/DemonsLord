using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace DemonsLord.CombatSystem
{
    public sealed class BattleSession : IDisposable
    {
        private static readonly Vector2Int PlayerSpawn = new Vector2Int(2, 4);
        private static readonly Vector2Int NpcSpawn = new Vector2Int(9, 4);

        public BattleSession(BattleWorldContext context, CombatTuning tuning)
        {
            Context = context ?? new BattleWorldContext();
            Tuning = tuning != null ? tuning : ScriptableObject.CreateInstance<CombatTuning>();

            if (Context.Player == null)
            {
                Context.Player = new BattleParticipantSetup { UnitId = "player", DisplayName = "Player" };
            }

            if (Context.Npc == null)
            {
                Context.Npc = new BattleParticipantSetup { UnitId = "npc", DisplayName = "NPC" };
            }

            Board = new BattleBoard();
            PlayerUnit = new UnitRuntime(Context.Player, BattleSide.Player, PlayerSpawn);
            NpcUnit = new UnitRuntime(Context.Npc, BattleSide.Npc, NpcSpawn);

            State = new ReactiveProperty<BattleSessionState>(BattleSessionState.Idle);
            CurrentSide = new ReactiveProperty<BattleSide>(BattleSide.Player);
            CurrentUnit = new ReactiveProperty<UnitRuntime>(null);
            SelectedUnit = new ReactiveProperty<UnitRuntime>(null);
            MoveHighlights = new ReactiveProperty<IReadOnlyList<Vector2Int>>(Array.Empty<Vector2Int>());
            AttackHighlights = new ReactiveProperty<IReadOnlyList<Vector2Int>>(Array.Empty<Vector2Int>());
            UnitMoved = new Subject<UnitMovedEvent>();
            UnitDamaged = new Subject<UnitDamagedEvent>();
            UnitDied = new Subject<UnitDiedEvent>();
        }

        public BattleWorldContext Context { get; }
        public CombatTuning Tuning { get; }
        public BattleBoard Board { get; }
        public UnitRuntime PlayerUnit { get; }
        public UnitRuntime NpcUnit { get; }

        public ReactiveProperty<BattleSessionState> State { get; }
        public ReactiveProperty<BattleSide> CurrentSide { get; }
        public ReactiveProperty<UnitRuntime> CurrentUnit { get; }
        public ReactiveProperty<UnitRuntime> SelectedUnit { get; }
        public ReactiveProperty<IReadOnlyList<Vector2Int>> MoveHighlights { get; }
        public ReactiveProperty<IReadOnlyList<Vector2Int>> AttackHighlights { get; }
        public Subject<UnitMovedEvent> UnitMoved { get; }
        public Subject<UnitDamagedEvent> UnitDamaged { get; }
        public Subject<UnitDiedEvent> UnitDied { get; }

        public int GetInitiative(UnitRuntime unit)
        {
            return unit != null ? unit.GetInitiativeScore(BattleBoard.Width) : 0;
        }

        public void Start()
        {
            if (State.Value != BattleSessionState.Idle)
            {
                return;
            }

            State.Value = BattleSessionState.Starting;

            Board.TryPlace(PlayerUnit, PlayerSpawn);
            Board.TryPlace(NpcUnit, NpcSpawn);

            CombatAutoSave.Save(CreateSaveData());
            BeginTurn(BattleSide.Player);
        }

        public void SelectUnit(UnitRuntime unit)
        {
            SelectedUnit.Value = unit;
        }

        public bool TryMove(Vector2Int destination)
        {
            var actor = CurrentUnit.Value;
            if (!CanAct(actor) || !ContainsPosition(MoveHighlights.Value, destination))
            {
                return false;
            }

            var from = actor.Position;
            if (!Board.TryMove(actor, destination))
            {
                return false;
            }

            UnitMoved.OnNext(new UnitMovedEvent(actor, from, destination));
            CompleteAction(actor, BattleActionType.Move, false);
            return true;
        }

        public bool TryAttack(Vector2Int targetPosition)
        {
            var actor = CurrentUnit.Value;
            if (!CanAct(actor) || !ContainsPosition(AttackHighlights.Value, targetPosition))
            {
                return false;
            }

            var target = Board.GetUnitAt(targetPosition);
            if (target == null || !target.IsAlive || target.Side == actor.Side)
            {
                return false;
            }

            int baseDamage = Mathf.Max(1, actor.Attack - target.GetEffectiveDef(Tuning.DefendMultiplier));
            bool isCritical = UnityEngine.Random.value < Mathf.Clamp01(actor.Luc * Tuning.CriticalChancePerLuck);
            int damage = isCritical
                ? Mathf.Max(1, Mathf.RoundToInt(baseDamage * Tuning.CriticalDamageMultiplier))
                : baseDamage;

            target.ApplyDamage(damage);
            UnitDamaged.OnNext(new UnitDamagedEvent(actor, target, damage, isCritical));

            bool keepCurrentSide = false;
            if (!target.IsAlive)
            {
                Board.Remove(target);
                UnitDied.OnNext(new UnitDiedEvent(target, actor));

                if (!HasAliveUnit(target.Side))
                {
                    Finish(BattleSessionState.Finished);
                    return true;
                }

                keepCurrentSide = true;
            }

            CompleteAction(actor, BattleActionType.Attack, keepCurrentSide);
            return true;
        }

        public bool TryDefend()
        {
            var actor = CurrentUnit.Value;
            if (!CanAct(actor))
            {
                return false;
            }

            actor.SetDefending();
            CompleteAction(actor, BattleActionType.Defend, false);
            return true;
        }

        public bool TryWait()
        {
            var actor = CurrentUnit.Value;
            if (!CanAct(actor))
            {
                return false;
            }

            CompleteAction(actor, BattleActionType.Wait, false);
            return true;
        }

        public bool TryEscape()
        {
            var actor = CurrentUnit.Value;
            if (!CanAct(actor))
            {
                return false;
            }

            float chance = CalculateEscapeChance(actor.Side);
            bool escaped = UnityEngine.Random.value <= chance;
            if (escaped)
            {
                Finish(BattleSessionState.Escaped);
                return true;
            }

            CompleteAction(actor, BattleActionType.Escape, false);
            return false;
        }

        public bool ExecuteNpcTurn()
        {
            if (State.Value != BattleSessionState.NpcTurn || CurrentSide.Value != BattleSide.Npc || CurrentUnit.Value == null)
            {
                return false;
            }

            var target = PlayerUnit;
            if (target == null || !target.IsAlive)
            {
                Finish(BattleSessionState.Finished);
                return false;
            }

            var attackHighlights = Board.GetAttackHighlights(NpcUnit);
            if (attackHighlights.Count > 0)
            {
                return TryAttack(attackHighlights[0]);
            }

            var move = GetBestMoveTowards(NpcUnit, target);
            if (move.HasValue)
            {
                return TryMove(move.Value);
            }

            return TryWait();
        }

        public CombatSaveData CreateSaveData()
        {
            return new CombatSaveData
            {
                savedAt = DateTime.UtcNow.ToString("o"),
                tileId = Context.TileId,
                biomeId = Context.BiomeId,
                state = State.Value,
                currentSide = CurrentSide.Value,
                player = CaptureUnit(PlayerUnit),
                npc = CaptureUnit(NpcUnit)
            };
        }

        public void Dispose()
        {
            State.Dispose();
            CurrentSide.Dispose();
            CurrentUnit.Dispose();
            SelectedUnit.Dispose();
            MoveHighlights.Dispose();
            AttackHighlights.Dispose();
            UnitMoved.Dispose();
            UnitDamaged.Dispose();
            UnitDied.Dispose();
        }

        private void BeginTurn(BattleSide side)
        {
            if (State.Value == BattleSessionState.Finished || State.Value == BattleSessionState.Escaped)
            {
                return;
            }

            var unit = GetUnit(side);
            if (unit == null || !unit.IsAlive)
            {
                Finish(BattleSessionState.Finished);
                return;
            }

            unit.BeginTurn();
            CurrentSide.Value = side;
            CurrentUnit.Value = unit;
            SelectedUnit.Value = unit;
            State.Value = side == BattleSide.Player ? BattleSessionState.PlayerTurn : BattleSessionState.NpcTurn;
            RefreshHighlights(unit);

            if (side == BattleSide.Npc)
            {
                ExecuteNpcTurn();
            }
        }

        private void CompleteAction(UnitRuntime actor, BattleActionType actionType, bool keepCurrentSide)
        {
            if (actor == null || State.Value == BattleSessionState.Finished || State.Value == BattleSessionState.Escaped)
            {
                return;
            }

            RefreshHighlights(null);

            if (keepCurrentSide)
            {
                BeginTurn(actor.Side);
                return;
            }

            BeginTurn(GetOppositeSide(actor.Side));
        }

        private void Finish(BattleSessionState finalState)
        {
            State.Value = finalState;
            RefreshHighlights(null);
            CurrentUnit.Value = null;
            SelectedUnit.Value = null;
            CombatAutoSave.Save(CreateSaveData());
        }

        private void RefreshHighlights(UnitRuntime unit)
        {
            MoveHighlights.Value = unit != null ? Board.GetMoveHighlights(unit) : Array.Empty<Vector2Int>();
            AttackHighlights.Value = unit != null ? Board.GetAttackHighlights(unit) : Array.Empty<Vector2Int>();
        }

        private UnitRuntime GetUnit(BattleSide side)
        {
            return side == BattleSide.Player ? PlayerUnit : NpcUnit;
        }

        private bool HasAliveUnit(BattleSide side)
        {
            var unit = GetUnit(side);
            return unit != null && unit.IsAlive;
        }

        private BattleSide GetOppositeSide(BattleSide side)
        {
            return side == BattleSide.Player ? BattleSide.Npc : BattleSide.Player;
        }

        private bool CanAct(UnitRuntime actor)
        {
            return actor != null
                   && actor.IsAlive
                   && CurrentUnit.Value == actor
                   && (State.Value == BattleSessionState.PlayerTurn || State.Value == BattleSessionState.NpcTurn);
        }

        private float CalculateEscapeChance(BattleSide escapingSide)
        {
            float playerLuc = PlayerUnit != null ? PlayerUnit.Luc : 0f;
            float npcLuc = NpcUnit != null ? NpcUnit.Luc : 0f;

            if (escapingSide == BattleSide.Player)
            {
                float denominator = playerLuc + npcLuc + Tuning.PlayerEscapeBonus;
                if (denominator <= 0f)
                {
                    return 0.5f;
                }

                return Mathf.Clamp01((playerLuc + Tuning.PlayerEscapeBonus) / denominator);
            }

            float npcDenominator = npcLuc + playerLuc + Tuning.PlayerEscapeBonus;
            if (npcDenominator <= 0f)
            {
                return 0.5f;
            }

            return Mathf.Clamp01(npcLuc / npcDenominator);
        }

        private Vector2Int? GetBestMoveTowards(UnitRuntime actor, UnitRuntime target)
        {
            var candidates = Board.GetMoveHighlights(actor);
            if (candidates.Count == 0)
            {
                return null;
            }

            Vector2Int best = candidates[0];
            int bestDistance = ManhattanDistance(best, target.Position);

            for (int i = 1; i < candidates.Count; i++)
            {
                int distance = ManhattanDistance(candidates[i], target.Position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidates[i];
                }
            }

            return best;
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static bool ContainsPosition(IReadOnlyList<Vector2Int> positions, Vector2Int target)
        {
            if (positions == null)
            {
                return false;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i] == target)
                {
                    return true;
                }
            }

            return false;
        }

        private static UnitSaveData CaptureUnit(UnitRuntime unit)
        {
            if (unit == null)
            {
                return null;
            }

            var highStats = new UnitHighStatsData
            {
                MaxHP = unit.HighStats.MaxHP,
                HP = unit.HighStats.HP,
                Mana = unit.HighStats.Mana,
                Ult = unit.HighStats.Ult,
                Level = unit.HighStats.Level
            };

            return new UnitSaveData
            {
                unitId = unit.UnitId,
                displayName = unit.DisplayName,
                side = unit.Side,
                position = unit.Position,
                highStats = highStats,
                isAlive = unit.IsAlive,
                isDefending = unit.IsDefending
            };
        }
    }

    public sealed class CombatService : IDisposable
    {
        public BattleSession CurrentSession { get; private set; }

        public BattleSession StartBattle(BattleWorldContext context, CombatTuning tuning)
        {
            Dispose();
            CurrentSession = new BattleSession(context, tuning);
            CurrentSession.Start();
            return CurrentSession;
        }

        public void Dispose()
        {
            if (CurrentSession == null)
            {
                return;
            }

            CurrentSession.Dispose();
            CurrentSession = null;
        }
    }
}
