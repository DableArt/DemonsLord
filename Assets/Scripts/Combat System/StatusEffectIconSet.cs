using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectIconSet", menuName = "SO/Status Effect Icon Set")]
public class StatusEffectIconSet : ScriptableObject
{
    public List<StatusEffectIconEntry> entries = new List<StatusEffectIconEntry>();

    public Sprite GetIcon(StatusEffectType type)
    {
        foreach (var e in entries)
            if (e.type == type)
                return e.icon;
        return null;
    }
}

[Serializable]
public class StatusEffectIconEntry
{
    public StatusEffectType type;
    public Sprite icon;
}
