using System;

public enum TerrainType
{
    Normal,
    Forest,
    Water,
    Mountain,
    Swamp,
    Lava,
    Ice,
    Sand,
    Rubble,
    MagicField,
    Trap,
    Obstacle
}

public enum BiomeType
{
    DarkForest,
    CursedSwamp,
    FireLands,
    IceWastes,
    BloodPlains,
    EnchantedGrove,
    ShadowDesert,
    MountainFortress,
    DeadLands,
    EvilLands,
    Hell,
    Plains,
    Desert
}

public enum UnitRank
{
    R,
    SR,
    SSR,
    UR,
    LR
}

public enum UnitHabitatType
{
    Ground,
    Air,
    Water,
    Underground,
    Ethereal
}

public enum UnitSize
{
    Small = 1,
    Large = 2
}

public enum MagicSchool
{
    Fire,
    Ice,
    Lightning,
    Dark,
    Light,
    Earth,
    Air,
    Time
}

public enum DamageType
{
    Physical,
    Magical,
    Fire,
    Ice,
    Lightning,
    Dark,
    Light,
    Earth,
    Air,
    Time,
    True
}

public enum SpellTargetType
{
    Enemy,
    Ally,
    Self,
    Cell,
    AllEnemies,
    AllAllies
}

public enum AIBehaviourType
{
    Aggressive,
    Defensive,
    Tactical
}

public enum AbilityType
{
    Passive = 0,
    Active = 1,
    Ultimate = 2
}

public enum BattlePhase
{
    Preparation,
    Initiative,
    PlayerTurn,
    EnemyTurn,
    Reaction,
    RoundEnd,
    Won,
    Lost
}
