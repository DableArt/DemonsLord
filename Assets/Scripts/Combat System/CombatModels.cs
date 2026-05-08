using System;
using UnityEngine;

namespace DemonsLord.CombatSystem
{
    public enum BattleSide
    {
        Player = 0,
        Npc = 1
    }

    public enum BattleSessionState
    {
        Idle = 0,
        Starting = 1,
        PlayerTurn = 2,
        NpcTurn = 3,
        Finished = 4,
        Escaped = 5
    }

    public enum BattleActionType
    {
        None = 0,
        Move = 1,
        Attack = 2,
        Defend = 3,
        Wait = 4,
        Escape = 5
    }

    [Serializable]
    public class UnitHighStatsData
    {
        [Min(1)] public int MaxHP = 10;
        [Min(0)] public int HP = 10;
        [Min(0)] public int Mana = 0;
        [Min(0)] public int Ult = 0;
        [Min(1)] public int Level = 1;

        public void EnsureValid()
        {
            MaxHP = Mathf.Max(1, MaxHP);
            HP = Mathf.Clamp(HP, 0, MaxHP);
            Mana = Mathf.Max(0, Mana);
            Ult = Mathf.Max(0, Ult);
            Level = Mathf.Max(1, Level);
        }
    }

    [Serializable]
    public class BattleParticipantSetup
    {
        public string UnitId = "unit";
        public string DisplayName = "Unit";
        public UnitStatsDefinition StatsDefinition;
        public UnitHighStatsData HighStats = new UnitHighStatsData();
    }

    [Serializable]
    public class BattleWorldContext
    {
        public string TileId = "grass";
        public string BiomeId = "plains";
        public BattleParticipantSetup Player = new BattleParticipantSetup
        {
            UnitId = "player",
            DisplayName = "Player"
        };
        public BattleParticipantSetup Npc = new BattleParticipantSetup
        {
            UnitId = "npc",
            DisplayName = "NPC"
        };
    }

    [Serializable]
    public class UnitSaveData
    {
        public string unitId;
        public string displayName;
        public BattleSide side;
        public Vector2Int position;
        public UnitHighStatsData highStats = new UnitHighStatsData();
        public bool isAlive;
        public bool isDefending;
    }

    [Serializable]
    public class CombatSaveData
    {
        public string savedAt;
        public string tileId;
        public string biomeId;
        public BattleSessionState state;
        public BattleSide currentSide;
        public UnitSaveData player;
        public UnitSaveData npc;
    }

    public struct UnitMovedEvent
    {
        public UnitMovedEvent(UnitRuntime unit, Vector2Int from, Vector2Int to)
        {
            Unit = unit;
            From = from;
            To = to;
        }

        public UnitRuntime Unit { get; }
        public Vector2Int From { get; }
        public Vector2Int To { get; }
    }

    public struct UnitDamagedEvent
    {
        public UnitDamagedEvent(UnitRuntime source, UnitRuntime target, int damage, bool isCritical)
        {
            Source = source;
            Target = target;
            Damage = damage;
            IsCritical = isCritical;
        }

        public UnitRuntime Source { get; }
        public UnitRuntime Target { get; }
        public int Damage { get; }
        public bool IsCritical { get; }
    }

    public struct UnitDiedEvent
    {
        public UnitDiedEvent(UnitRuntime unit, UnitRuntime killer)
        {
            Unit = unit;
            Killer = killer;
        }

        public UnitRuntime Unit { get; }
        public UnitRuntime Killer { get; }
    }
}
