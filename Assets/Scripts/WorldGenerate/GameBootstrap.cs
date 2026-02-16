using System.Collections;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public WorldSettings settings;
    public ChunkManager chunkManager;

    public GameObject playerPrefab;
    public Camera cam;

    public Vector3 playerSpawnWorld = new Vector3(0, 0, 0);

    public int preloadRadiusChunks = 2; // заранее сгенерировать квадрат вокруг спавна

    IEnumerator Start()
    {
        // 1) Лоадинг: предзагрузка чанков вокруг спавна
        var spawnChunk = new Vector2Int(
            Mathf.FloorToInt(playerSpawnWorld.x / settings.chunkSize),
            Mathf.FloorToInt(playerSpawnWorld.y / settings.chunkSize)
        );

        // Временно “симулируем камеру” на спавне
        cam.transform.position = new Vector3(playerSpawnWorld.x, playerSpawnWorld.y, cam.transform.position.z);

        // прогреть кадр, чтобы всё создалось
        yield return null;

        // Прелоад: просто заставим ChunkManager прогрузить нужный радиус
        int oldLoadR = settings.loadRadiusChunks;
        settings.loadRadiusChunks = preloadRadiusChunks;
        chunkManager.cam = cam;
        chunkManager.enabled = true;
        chunkManager.LateUpdate(); // ручной вызов один раз
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


        // 3) Камеру на игрока (если используешь свой CameraMovement)
        var follow = cam.GetComponent<CameraMovement>();
        if (follow != null) follow.target = player.transform;

        // 4) Старт геймплея: теперь чанк-стриминг работает от реальной камеры
    }
}
