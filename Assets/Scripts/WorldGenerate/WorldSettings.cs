using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "World/WorldSettings")]
public class WorldSettings : ScriptableObject
{
    [Header("Chunk")]
    public int chunkSize = 32;         // 32x32 �����
    public int pixelsPerUnit = 16;     // ���� ����� ��� �������� (�� �����������)

    [Header("Streaming")]
    public int loadRadiusChunks = 2;   // ������� ������ ������ ������ ������
    public int unloadRadiusChunks = 4; // ������ � ���������

    [Header("Noise")]
    public int seed = 12345;
    public float noiseScale = 0.06f;
    [Range(0f, 1f)] public float waterThreshold = 0.45f;

    [Header("Tiles")]
    public TileBase groundTile;
    public TileBase waterTile;

    [Header("NPC Spawning")]
    public GameObject[] npcPrefabs;
    [Min(0)] public int npcMinCount = 3;
    [Min(0)] public int npcMaxCount = 10;
}
