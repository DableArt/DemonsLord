using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "SO/Ability")]
public class AbilityBase : ScriptableObject
{
    public string abilityName;
    public AbilityType abilityType;
    public string description;
    public int power;
    public int mpCost;
    public int cooldown;
    public int range = 1;
    public int areaOfEffect;
    public MagicSchool school;
    public DamageType damageType = DamageType.Physical;
    public SpellTargetType targetType = SpellTargetType.Enemy;
    public Sprite icon;
}
