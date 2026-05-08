using UnityEngine;

namespace DemonsLord.CombatSystem
{
    public class UnitRuntime
    {
        public UnitRuntime(BattleParticipantSetup setup, BattleSide side, Vector2Int spawnPosition)
        {
            setup = setup ?? new BattleParticipantSetup();
            Side = side;
            UnitId = string.IsNullOrWhiteSpace(setup.UnitId) ? side.ToString().ToLowerInvariant() : setup.UnitId;
            DisplayName = string.IsNullOrWhiteSpace(setup.DisplayName) ? UnitId : setup.DisplayName;
            StatsDefinition = setup.StatsDefinition;
            HighStats = setup.HighStats ?? new UnitHighStatsData();
            HighStats.EnsureValid();
            Position = spawnPosition;
        }

        public string UnitId { get; }
        public string DisplayName { get; }
        public BattleSide Side { get; }
        public UnitStatsDefinition StatsDefinition { get; }
        public UnitHighStatsData HighStats { get; }
        public Vector2Int Position { get; private set; }
        public bool IsDefending { get; private set; }

        public bool IsAlive
        {
            get { return HighStats != null && HighStats.HP > 0; }
        }

        public int Attack
        {
            get { return StatsDefinition != null ? StatsDefinition.Attack : 0; }
        }

        public int Ag
        {
            get { return StatsDefinition != null ? StatsDefinition.Ag : 0; }
        }

        public int Luc
        {
            get { return StatsDefinition != null ? StatsDefinition.Luc : 0; }
        }

        public int Int
        {
            get { return StatsDefinition != null ? StatsDefinition.Int : 0; }
        }

        public int Def
        {
            get { return StatsDefinition != null ? StatsDefinition.Def : 0; }
        }

        public void BeginTurn()
        {
            IsDefending = false;
            HighStats.EnsureValid();
        }

        public void SetPosition(Vector2Int position)
        {
            Position = position;
        }

        public void SetDefending()
        {
            IsDefending = true;
        }

        public int GetEffectiveDef(float defendMultiplier)
        {
            return Mathf.Max(0, Mathf.RoundToInt(Def * (IsDefending ? defendMultiplier : 1f)));
        }

        public int GetInitiativeScore(int boardWidth)
        {
            int progressToEnemy = Side == BattleSide.Player ? Position.x : (boardWidth - 1 - Position.x);
            return Ag * 10 + progressToEnemy;
        }

        public void ApplyDamage(int damage)
        {
            if (HighStats == null)
            {
                return;
            }

            HighStats.HP = Mathf.Max(0, HighStats.HP - Mathf.Max(0, damage));
        }
    }
}
