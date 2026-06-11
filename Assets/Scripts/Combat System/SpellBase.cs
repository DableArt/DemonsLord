using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "SO/Spell")]
public class SpellBase : ScriptableObject
{
    public string spellName;
    public MagicSchool school;
    public int mpCost = 5;
    public int range = 3;
    public int areaOfEffect;
    public int power = 10;
    public DamageType damageType = DamageType.Magical;
    public SpellTargetType targetType = SpellTargetType.Enemy;
    public string description;
    public List<StatusEffectData> statusEffects = new List<StatusEffectData>();

    public static SpellBase Create(string name, MagicSchool school, int mpCost,
        int range, int power, DamageType damageType, SpellTargetType targetType,
        int aoe = 0)
    {
        var spell = CreateInstance<SpellBase>();
        spell.spellName = name;
        spell.school = school;
        spell.mpCost = mpCost;
        spell.range = range;
        spell.power = power;
        spell.damageType = damageType;
        spell.targetType = targetType;
        spell.areaOfEffect = aoe;
        spell.description = name;
        return spell;
    }
}

[System.Serializable]
public struct StatusEffectData
{
    public StatusEffectType type;
    [Range(0, 100)] public int applyChance;
    public int duration;
    public int potency;
}

public static class DefaultSpells
{
    public static SpellBase Fireball
    {
        get
        {
            var s = SpellBase.Create("Fireball", MagicSchool.Fire, 5, 3, 10, DamageType.Fire, SpellTargetType.Enemy);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Burn, applyChance = 50, duration = 2, potency = 5 });
            return s;
        }
    }
    public static SpellBase IceBolt
    {
        get
        {
            var s = SpellBase.Create("Ice Bolt", MagicSchool.Ice, 4, 3, 8, DamageType.Ice, SpellTargetType.Enemy);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Slow, applyChance = 60, duration = 2, potency = 3 });
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Freeze, applyChance = 20, duration = 1, potency = 0 });
            return s;
        }
    }
    public static SpellBase Lightning
    {
        get
        {
            var s = SpellBase.Create("Lightning", MagicSchool.Lightning, 6, 4, 15, DamageType.Lightning, SpellTargetType.Enemy);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Stun, applyChance = 30, duration = 1, potency = 0 });
            return s;
        }
    }
    public static SpellBase DarkBolt
    {
        get
        {
            var s = SpellBase.Create("Dark Bolt", MagicSchool.Dark, 5, 3, 12, DamageType.Dark, SpellTargetType.Enemy);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Weaken, applyChance = 50, duration = 2, potency = 5 });
            return s;
        }
    }
    public static SpellBase LightHeal
    {
        get
        {
            var s = SpellBase.Create("Light Heal", MagicSchool.Light, 4, 2, 15, DamageType.Light, SpellTargetType.Ally);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Regeneration, applyChance = 40, duration = 2, potency = 4 });
            return s;
        }
    }
    public static SpellBase EarthSpike
    {
        get
        {
            var s = SpellBase.Create("Earth Spike", MagicSchool.Earth, 5, 3, 10, DamageType.Earth, SpellTargetType.Enemy);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Slow, applyChance = 40, duration = 1, potency = 3 });
            return s;
        }
    }
    public static SpellBase WindBlade =>
        SpellBase.Create("Wind Blade", MagicSchool.Air, 4, 3, 8, DamageType.Air, SpellTargetType.Enemy);
    public static SpellBase Haste
    {
        get
        {
            var s = SpellBase.Create("Haste", MagicSchool.Time, 6, 3, 0, DamageType.Time, SpellTargetType.Ally);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Haste, applyChance = 100, duration = 3, potency = 5 });
            return s;
        }
    }
    public static SpellBase FireStorm
    {
        get
        {
            var s = SpellBase.Create("Fire Storm", MagicSchool.Fire, 10, 4, 12, DamageType.Fire, SpellTargetType.Enemy, 1);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Burn, applyChance = 70, duration = 3, potency = 8 });
            return s;
        }
    }
    public static SpellBase DarkNova
    {
        get
        {
            var s = SpellBase.Create("Dark Nova", MagicSchool.Dark, 12, 4, 15, DamageType.Dark, SpellTargetType.Enemy, 1);
            s.statusEffects.Add(new StatusEffectData { type = StatusEffectType.Weaken, applyChance = 60, duration = 3, potency = 8 });
            return s;
        }
    }
}
