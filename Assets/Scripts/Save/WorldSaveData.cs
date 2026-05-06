using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldSaveData
{
    public int seed;
    public Vector3 playerSpawnPosition;
    public List<Vector3> npcPositions = new List<Vector3>();
    public string savedAt;
}
