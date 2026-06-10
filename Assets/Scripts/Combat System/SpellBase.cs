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

public static class DefaultSpells
{
    public static SpellBase Fireball =>
        SpellBase.Create("Fireball", MagicSchool.Fire, 5, 3, 10, DamageType.Fire, SpellTargetType.Enemy);
    public static SpellBase IceBolt =>
        SpellBase.Create("Ice Bolt", MagicSchool.Ice, 4, 3, 8, DamageType.Ice, SpellTargetType.Enemy);
    public static SpellBase Lightning =>
        SpellBase.Create("Lightning", MagicSchool.Lightning, 6, 4, 15, DamageType.Lightning, SpellTargetType.Enemy);
    public static SpellBase DarkBolt =>
        SpellBase.Create("Dark Bolt", MagicSchool.Dark, 5, 3, 12, DamageType.Dark, SpellTargetType.Enemy);
    public static SpellBase LightHeal =>
        SpellBase.Create("Light Heal", MagicSchool.Light, 4, 2, 15, DamageType.Light, SpellTargetType.Ally);
    public static SpellBase EarthSpike =>
        SpellBase.Create("Earth Spike", MagicSchool.Earth, 5, 3, 10, DamageType.Earth, SpellTargetType.Enemy);
    public static SpellBase WindBlade =>
        SpellBase.Create("Wind Blade", MagicSchool.Air, 4, 3, 8, DamageType.Air, SpellTargetType.Enemy);
    public static SpellBase Haste =>
        SpellBase.Create("Haste", MagicSchool.Time, 6, 3, 0, DamageType.Time, SpellTargetType.Ally);
    public static SpellBase FireStorm =>
        SpellBase.Create("Fire Storm", MagicSchool.Fire, 10, 4, 12, DamageType.Fire, SpellTargetType.Enemy, 1);
    public static SpellBase DarkNova =>
        SpellBase.Create("Dark Nova", MagicSchool.Dark, 12, 4, 15, DamageType.Dark, SpellTargetType.Enemy, 1);
}
