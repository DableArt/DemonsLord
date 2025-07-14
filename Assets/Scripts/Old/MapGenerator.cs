using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic; // Required for List

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 128;        // Ширина карты
    public int height = 128;       // Высота карты
    public float scale = 20f;       // Масштаб шума
    public float offset_x = 0f;     // Смещение шума по X
    public float offset_y = 0f;     // Смещение шума по Y
    public int depth = 3;
    public bool useSmoothTransitions = true; // Использовать плавные переходы между тайлами

    [Header("Ground Tile Settings")]
    public List<TileBase> groundTiles; // Список тайлов земли
    [Range(0f, 1f)]
    public List<float> groundTileProbabilities; // Вероятности выбора каждого тайла земли

    [Header("Water Tile Settings")]
    public List<TileBase> waterTiles; // Список тайлов воды
    [Range(0f, 1f)]
    public List<float> waterTileProbabilities; // Вероятности выбора каждого тайла воды

    [Header("Water Level")]
    public float waterLevel = 0.5f;  // Уровень воды (0-1)

    [Header("Transition Tiles")]
    public TileBase groundWaterTransition_N; // Переход земля-вода (север)
    public TileBase groundWaterTransition_E; // Переход земля-вода (восток)
    public TileBase groundWaterTransition_S; // Переход земля-вода (юг)
    public TileBase groundWaterTransition_W; // Переход земля-вода (запад)
    public TileBase groundWaterTransition_NE; // Переход земля-вода (северо-восток)
    public TileBase groundWaterTransition_SE; // Переход земля-вода (юго-восток)
    public TileBase groundWaterTransition_SW; // Переход земля-вода (юго-запад)
    public TileBase groundWaterTransition_NW; // Переход земля-вода (северо-запад)

    [Header("Tilemap References")]
    public Tilemap tilemap;         // Ссылка на Tilemap

    void Start()
    {
        offset_x = Random.Range(0f, 99999f);
        offset_y = Random.Range(0f, 99999f);

        // Проверка корректности вероятностей
        if (groundTiles.Count != groundTileProbabilities.Count || waterTiles.Count != waterTileProbabilities.Count)
        {
            Debug.LogError("Количество тайлов и вероятностей не совпадает!");
            return;
        }
        GenerateMap();
    }

    void GenerateMap()
    {
        // Очищаем карту
        tilemap.ClearAllTiles();

        // Генерируем карту на основе шума
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = CalculateNoiseValue(x, y);

                // Определяем, какой тайл установить (вода или земля)
                TileBase tile = noiseValue < waterLevel ? GetRandomTile(waterTiles, waterTileProbabilities) : GetRandomTile(groundTiles, groundTileProbabilities);

                // Устанавливаем тайл на Tilemap
                tilemap.SetTile(new Vector3Int(x - width / 2, y - height / 2, depth), tile); // Центрируем карту
            }
        }

        if (useSmoothTransitions)
        {
            ApplySmoothTransitions();
        }
    }

    float CalculateNoiseValue(int x, int y)
    {
        float xCoord = (float)x / width * scale + offset_x;
        float yCoord = (float)y / height * scale + offset_y;

        // Используем Perlin Noise для генерации ландшафта
        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    // Функция для получения случайного тайла из списка с учетом вероятностей
    TileBase GetRandomTile(List<TileBase> tiles, List<float> probabilities)
    {
        if (tiles.Count == 0) return null; // Если нет тайлов, возвращаем null

        float totalProbability = 0f;
        foreach (float prob in probabilities)
        {
            totalProbability += prob;
        }
        if (totalProbability <= 0f) return tiles[Random.Range(0, tiles.Count)]; // если все вероятности равны 0

        float randomValue = Random.Range(0f, totalProbability); // Изменяем на totalProbability

        float cumulativeProbability = 0f;
        for (int i = 0; i < tiles.Count; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                return tiles[i];
            }
        }

        return tiles[tiles.Count - 1]; // Возвращаем последний тайл, если что-то пошло не так
    }

    // Применяем плавные переходы между тайлами земли и воды
    void ApplySmoothTransitions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int currentPos = new Vector3Int(x - width / 2, y - height / 2, depth);
                TileBase currentTile = tilemap.GetTile(currentPos);

                bool isGround = false;
                foreach (TileBase tile in groundTiles)
                {
                    if (currentTile == tile)
                    {
                        isGround = true;
                        break;
                    }
                }

                if (isGround) // Переходы только для земли
                {
                    TileBase transitionTile = GetTransitionTile(currentPos);
                    if (transitionTile != null)
                    {
                        tilemap.SetTile(currentPos, transitionTile);
                    }
                }
            }
        }
    }

    // Определяем, какой тайл перехода использовать
    TileBase GetTransitionTile(Vector3Int position)
    {
        bool n = IsWater(position + new Vector3Int(0, 1, 0));
        bool e = IsWater(position + new Vector3Int(1, 0, 0));
        bool s = IsWater(position + new Vector3Int(0, -1, 0));
        bool w = IsWater(position + new Vector3Int(-1, 0, 0));

        if (n && !e && !s && !w) return groundWaterTransition_N;
        if (!n && e && !s && !w) return groundWaterTransition_E;
        if (!n && !e && s && !w) return groundWaterTransition_S;
        if (!n && !e && !s && w) return groundWaterTransition_W;

        if (n && e && !s && !w) return groundWaterTransition_NE;
        if (!n && e && s && !w) return groundWaterTransition_SE;
        if (!n && !e && s && w) return groundWaterTransition_SW;
        if (n && !e && !s && w) return groundWaterTransition_NW;

        return null;
    }

    // Проверяем, является ли тайл водой
    bool IsWater(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        if (tile == null) return false; // Добавлена проверка на null

        foreach (TileBase waterTile in waterTiles)
        {
            if (tile == waterTile) return true;
        }
        return false;
    }

    // (Опционально) Функция для перегенерации карты
    public void RegenerateMap()
    {
        GenerateMap();
    }

}