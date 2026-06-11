[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int remainingDuration;
    public int potency;
    public string sourceName;

    public StatusEffect(StatusEffectType type, int duration, int potency, string source)
    {
        this.type = type;
        this.remainingDuration = duration;
        this.potency = potency;
        this.sourceName = source;
    }

    public bool IsExpired => remainingDuration <= 0;

    public bool IsDot =>
        type == StatusEffectType.Burn ||
        type == StatusEffectType.Poison ||
        type == StatusEffectType.Bleed;

    public bool PreventsAction =>
        type == StatusEffectType.Stun ||
        type == StatusEffectType.Freeze;
}
