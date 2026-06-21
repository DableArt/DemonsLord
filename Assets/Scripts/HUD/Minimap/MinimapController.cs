using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;
using WorldGenerate;

public class MinimapController : MonoBehaviour
{
    private const float MinimumWorldSize = 0.001f;

    [Header("References")]
    [SerializeField] private RectTransform mapPanel;
    [SerializeField] private Transform playerTransform;

    [Header("Marker Settings")]
    [SerializeField] private Color playerColor = Color.yellow;
    [SerializeField] private Color npcColor = Color.white;
    [SerializeField] private float playerMarkerSize = 20f;
    [SerializeField] private float npcMarkerSize = 10f;
    [SerializeField] private float markerBorderPadding = 4f;

    [Header("Custom Icons")]
    [SerializeField] private List<MinimapIconEntry> customIcons = new List<MinimapIconEntry>();

    [Header("Map Style")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Color groundColor = new Color(0.35f, 0.28f, 0.2f, 1f);
    [SerializeField] private Color shoreColor = new Color(0.7f, 0.75f, 0.5f, 1f);
    [SerializeField] private Color waterColor = new Color(0.15f, 0.35f, 0.55f, 1f);

    private readonly CompositeDisposable disposables = new CompositeDisposable();
    private readonly Dictionary<MinimapMarker, RectTransform> markerRects = new Dictionary<MinimapMarker, RectTransform>();
    private readonly Dictionary<Vector2Int, ChunkMapData> chunkDataByCoord = new Dictionary<Vector2Int, ChunkMapData>();
    private readonly Dictionary<string, MinimapIconEntry> customIconById = new Dictionary<string, MinimapIconEntry>();

    private RawImage backgroundImage;
    private Texture2D mapTexture;
    private Sprite playerSprite;
    private Sprite npcSprite;
    private Vector2 worldMin;
    private Vector2 worldSize;

    public static MinimapController Instance { get; private set; }

    public ReactiveProperty<Bounds> WorldBounds { get; } = new ReactiveProperty<Bounds>();

    public RectTransform MapPanel => mapPanel;
    public Transform PlayerTransform => playerTransform;
    public Color PlayerColor => playerColor;
    public Color NpcColor => npcColor;
    public float PlayerMarkerSize => playerMarkerSize;
    public float NpcMarkerSize => npcMarkerSize;
    public float MarkerBorderPadding => markerBorderPadding;
    public IReadOnlyList<MinimapIconEntry> CustomIcons => customIcons;
    public Color BackgroundColor => backgroundColor;
    public Color GroundColor => groundColor;
    public Color WaterColor => waterColor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildCustomIconLookup();

        playerSprite = GenerateTriangleSprite(Mathf.CeilToInt(playerMarkerSize), playerColor);
        npcSprite = GenerateCircleSprite(Mathf.CeilToInt(npcMarkerSize * 0.5f), npcColor);

        MinimapMarker.MarkerRegistrationChanged += OnMarkerRegistrationChanged;
        disposables.Add(WorldBounds.Subscribe(_ => RebuildBackground()));
    }

    private void Start()
    {
        foreach (var marker in MinimapMarker.RegisteredMarkers)
        {
            RegisterMarker(marker);
        }
    }

    private void Update()
    {
        if (mapPanel == null || worldSize.x <= 0f || worldSize.y <= 0f)
        {
            return;
        }

        UpdateMarkerPositions();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        MinimapMarker.MarkerRegistrationChanged -= OnMarkerRegistrationChanged;
        disposables.Dispose();

        if (mapTexture != null)
        {
            Destroy(mapTexture);
            mapTexture = null;
        }

        if (playerSprite != null)
        {
            Destroy(playerSprite.texture);
            Destroy(playerSprite);
        }

        if (npcSprite != null)
        {
            Destroy(npcSprite.texture);
            Destroy(npcSprite);
        }
    }

    public void SetWorldBounds(Vector2 min, Vector2 max)
    {
        worldMin = min;
        worldSize = new Vector2(
            Mathf.Max(MinimumWorldSize, max.x - min.x),
            Mathf.Max(MinimumWorldSize, max.y - min.y));

        Bounds bounds = new Bounds();
        bounds.SetMinMax(
            new Vector3(min.x, min.y, 0f),
            new Vector3(max.x, max.y, 0f));

        WorldBounds.Value = bounds;
    }

    public void RegisterChunk(Vector2Int coord, int chunkSize, TerrainType[,] data)
    {
        if (data == null)
        {
            return;
        }

        chunkDataByCoord[coord] = new ChunkMapData(chunkSize, data);

        if (mapTexture != null)
        {
            DrawChunk(coord, chunkSize, data);
            mapTexture.Apply(false);
        }
    }

    public void RegisterMarker(MinimapMarker marker)
    {
        if (marker == null || mapPanel == null || markerRects.ContainsKey(marker))
        {
            return;
        }

        GameObject markerObject = new GameObject($"MinimapMarker_{marker.name}", typeof(RectTransform), typeof(Image));
        markerObject.transform.SetParent(mapPanel, false);

        RectTransform markerRect = markerObject.GetComponent<RectTransform>();
        markerRect.anchorMin = new Vector2(0.5f, 0.5f);
        markerRect.anchorMax = new Vector2(0.5f, 0.5f);
        markerRect.pivot = new Vector2(0.5f, 0.5f);

        Image markerImage = markerObject.GetComponent<Image>();
        ConfigureMarkerVisual(marker, markerRect, markerImage);

        markerRects.Add(marker, markerRect);

        if (marker.MarkerType == MinimapMarkerType.Player && playerTransform == null)
        {
            playerTransform = marker.transform;
        }
    }

    public void UnregisterMarker(MinimapMarker marker)
    {
        if (marker == null)
        {
            return;
        }

        if (markerRects.TryGetValue(marker, out RectTransform markerRect) && markerRect != null)
        {
            Destroy(markerRect.gameObject);
        }

        markerRects.Remove(marker);
    }

    private void OnMarkerRegistrationChanged(MinimapMarker marker, bool isRegistered)
    {
        if (isRegistered)
        {
            RegisterMarker(marker);
            return;
        }

        UnregisterMarker(marker);
    }

    private void UpdateMarkerPositions()
    {
        Rect panelRect = mapPanel.rect;
        float halfWidth = Mathf.Max(0f, panelRect.width * 0.5f - markerBorderPadding);
        float halfHeight = Mathf.Max(0f, panelRect.height * 0.5f - markerBorderPadding);

        List<MinimapMarker> toRemove = null;

        foreach (var kv in markerRects)
        {
            MinimapMarker marker = kv.Key;
            RectTransform markerRect = kv.Value;

            if (marker == null || markerRect == null)
            {
                toRemove ??= new List<MinimapMarker>();
                toRemove.Add(marker);
                continue;
            }

            Vector2 panelPosition = WorldToPanelPosition(marker.transform.position, panelRect.size);
            panelPosition.x = Mathf.Clamp(panelPosition.x, -halfWidth, halfWidth);
            panelPosition.y = Mathf.Clamp(panelPosition.y, -halfHeight, halfHeight);
            markerRect.anchoredPosition = panelPosition;

            if (marker.MarkerType == MinimapMarkerType.Player && marker.RotateWithTransform)
            {
                Transform rotationSource = playerTransform != null ? playerTransform : marker.transform;
                markerRect.localRotation = Quaternion.Euler(0f, 0f, -rotationSource.eulerAngles.z);
            }
            else
            {
                markerRect.localRotation = Quaternion.identity;
            }
        }

        if (toRemove != null)
        {
            foreach (MinimapMarker marker in toRemove)
            {
                UnregisterMarker(marker);
            }
        }
    }

    private Vector2 WorldToPanelPosition(Vector3 worldPosition, Vector2 panelSize)
    {
        float normalizedX = (worldPosition.x - worldMin.x) / worldSize.x;
        float normalizedY = (worldPosition.y - worldMin.y) / worldSize.y;

        float panelX = normalizedX * panelSize.x - panelSize.x * 0.5f;
        float panelY = normalizedY * panelSize.y - panelSize.y * 0.5f;

        return new Vector2(panelX, panelY);
    }

    private void ConfigureMarkerVisual(MinimapMarker marker, RectTransform markerRect, Image markerImage)
    {
        markerImage.color = Color.white;

        switch (marker.MarkerType)
        {
            case MinimapMarkerType.Player:
                markerImage.sprite = playerSprite;
                markerRect.sizeDelta = Vector2.one * playerMarkerSize;
                break;
            case MinimapMarkerType.NPC:
                markerImage.sprite = npcSprite;
                markerRect.sizeDelta = Vector2.one * npcMarkerSize;
                break;
            case MinimapMarkerType.Custom:
                if (!string.IsNullOrEmpty(marker.CustomIconId) && customIconById.TryGetValue(marker.CustomIconId, out MinimapIconEntry entry) && entry.Sprite != null)
                {
                    markerImage.sprite = entry.Sprite;
                    markerImage.color = entry.Color;
                    markerRect.sizeDelta = entry.Size;
                }
                else
                {
                    markerImage.sprite = npcSprite;
                    markerRect.sizeDelta = Vector2.one * npcMarkerSize;
                }
                break;
        }
    }

    private void BuildCustomIconLookup()
    {
        customIconById.Clear();

        foreach (MinimapIconEntry entry in customIcons)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || entry.Sprite == null)
            {
                continue;
            }

            customIconById[entry.Id] = entry;
        }
    }

    private void RebuildBackground()
    {
        if (mapPanel == null)
        {
            return;
        }

        EnsureBackgroundImage();

        int width = Mathf.Max(1, Mathf.CeilToInt(worldSize.x));
        int height = Mathf.Max(1, Mathf.CeilToInt(worldSize.y));

        if (mapTexture != null)
        {
            Destroy(mapTexture);
        }

        mapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }

        mapTexture.SetPixels(pixels);

        foreach (var chunkPair in chunkDataByCoord)
        {
            DrawChunk(chunkPair.Key, chunkPair.Value.ChunkSize, chunkPair.Value.Data);
        }

        mapTexture.Apply(false);
        backgroundImage.texture = mapTexture;
        backgroundImage.color = Color.white;
    }

    private void EnsureBackgroundImage()
    {
        if (backgroundImage != null)
        {
            return;
        }

        GameObject backgroundObject = new GameObject("MinimapBackground", typeof(RectTransform), typeof(RawImage));
        backgroundObject.transform.SetParent(mapPanel, false);
        backgroundObject.transform.SetAsFirstSibling();

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        backgroundImage = backgroundObject.GetComponent<RawImage>();
        backgroundImage.raycastTarget = false;
    }

    private void DrawChunk(Vector2Int coord, int chunkSize, TerrainType[,] data)
    {
        if (mapTexture == null)
        {
            return;
        }

        int textureWidth = mapTexture.width;
        int textureHeight = mapTexture.height;

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int worldX = coord.x * chunkSize + x;
                int worldY = coord.y * chunkSize + y;

                int px = Mathf.FloorToInt(worldX - worldMin.x);
                int py = Mathf.FloorToInt(worldY - worldMin.y);

                if (px < 0 || py < 0 || px >= textureWidth || py >= textureHeight)
                {
                    continue;
                }

                mapTexture.SetPixel(px, py, data[x, y] == TerrainType.Water ? waterColor : 
                    data[x, y] == TerrainType.Shore ? shoreColor : groundColor);
            }
        }
    }

    private Sprite GenerateTriangleSprite(int size, Color color)
    {
        int safeSize = Mathf.Max(4, size);
        Texture2D texture = new Texture2D(safeSize, safeSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] pixels = new Color[safeSize * safeSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = clear;
        }

        texture.SetPixels(pixels);

        Vector2 a = new Vector2(safeSize * 0.5f, safeSize - 1f);
        Vector2 b = new Vector2(1f, 1f);
        Vector2 c = new Vector2(safeSize - 2f, 1f);

        float minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
        float maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
        float minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
        float maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));

        for (int y = Mathf.FloorToInt(minY); y <= Mathf.CeilToInt(maxY); y++)
        {
            for (int x = Mathf.FloorToInt(minX); x <= Mathf.CeilToInt(maxX); x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                if (PointInTriangle(p, a, b, c))
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        texture.Apply(false);
        return Sprite.Create(texture, new Rect(0f, 0f, safeSize, safeSize), new Vector2(0.5f, 0.5f), safeSize);
    }

    private Sprite GenerateCircleSprite(int radius, Color color)
    {
        int safeRadius = Mathf.Max(1, radius);
        int diameter = safeRadius * 2;

        Texture2D texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new Color(0f, 0f, 0f, 0f);
        Vector2 center = new Vector2(safeRadius - 0.5f, safeRadius - 0.5f);
        float sqrRadius = safeRadius * safeRadius;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                Vector2 delta = new Vector2(x, y) - center;
                texture.SetPixel(x, y, delta.sqrMagnitude <= sqrRadius ? color : clear);
            }
        }

        texture.Apply(false);
        return Sprite.Create(texture, new Rect(0f, 0f, diameter, diameter), new Vector2(0.5f, 0.5f), diameter);
    }

    private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);

        bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
        bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;

        return !(hasNeg && hasPos);
    }

    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private readonly struct ChunkMapData
    {
        public ChunkMapData(int chunkSize, TerrainType[,] data)
        {
            ChunkSize = chunkSize;
            Data = data;
        }

        public int ChunkSize { get; }
        public TerrainType[,] Data { get; }
    }
}
