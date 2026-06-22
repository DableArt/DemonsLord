using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGenerate;

public class GameBootstrap : MonoBehaviour
{
    public WorldSettings settings;
    public ChunkManager chunkManager;

    public GameObject playerPrefab;
    public Camera cam;

    public Vector3 playerSpawnWorld = new Vector3(0, 0, 0);

    public int preloadRadiusChunks = 2;

    IEnumerator Start()
    {
        // Ensure the WoodCounter singleton is present in the scene
        if (WoodCounter.Instance == null)
        {
            var wcGO = new GameObject("WoodCounterManager");
            wcGO.AddComponent<WoodCounter>();
        }

        cam.transform.position = new Vector3(playerSpawnWorld.x, playerSpawnWorld.y, cam.transform.position.z);

        yield return null;

        int oldLoadR = settings.loadRadiusChunks;
        settings.loadRadiusChunks = preloadRadiusChunks;
        chunkManager.cam = cam;
        chunkManager.enabled = true;
        chunkManager.LateUpdate();
        yield return null;
        settings.loadRadiusChunks = oldLoadR;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            playerObj = Instantiate(playerPrefab, playerSpawnWorld, Quaternion.identity);
        }
        else
        {
            playerObj.transform.position = playerSpawnWorld;
        }

        var player = playerObj.transform;

        var follow = cam.GetComponent<CameraMovement>();
        if (follow != null) follow.target = player.transform;

        // Spawn NPCs only on Ground tiles
        List<Vector3> npcPositions = SpawnNPCs();

        // Auto-save world state to JSON
        var saveData = new WorldSaveData
        {
            seed = settings.seed,
            playerSpawnPosition = playerSpawnWorld,
            npcPositions = npcPositions,
            savedAt = System.DateTime.UtcNow.ToString("o")
        };
        AutoSave.Save(saveData);
    }

    private List<Vector3> SpawnNPCs()
    {
        var positions = new List<Vector3>();

        if (settings.npcPrefabs == null || settings.npcPrefabs.Length == 0)
            return positions;

        int minCount = Mathf.Min(settings.npcMinCount, settings.npcMaxCount);
        int maxCount = Mathf.Max(settings.npcMinCount, settings.npcMaxCount);
        int count = Random.Range(minCount, maxCount + 1);
        int spawnRadius = preloadRadiusChunks * settings.chunkSize;

        // Reuse the same generator to validate tile types deterministically
        var generator = new WorldGenerator(settings);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = settings.npcPrefabs[Random.Range(0, settings.npcPrefabs.Length)];
            if (prefab == null) continue;

            // Pick a random Ground position within the preloaded world area
            const int maxAttempts = 20;
            bool found = false;
            Vector3 pos = Vector3.zero;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float rx = Random.Range(-spawnRadius, spawnRadius);
                float ry = Random.Range(-spawnRadius, spawnRadius);
                Vector3 candidate = new Vector3(playerSpawnWorld.x + rx, playerSpawnWorld.y + ry, 0f);

                if (generator.GetTerrainType(candidate) == TerrainType.Ground)
                {
                    pos = candidate;
                    found = true;
                    break;
                }
            }

            if (!found) continue;

            Instantiate(prefab, pos, Quaternion.identity);
            positions.Add(pos);
        }

        return positions;
    }
}
