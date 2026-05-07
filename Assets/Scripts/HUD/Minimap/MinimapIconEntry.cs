using UnityEngine;

[System.Serializable]
public class MinimapIconEntry
{
    [SerializeField] private string id;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private Vector2 size = new Vector2(12f, 12f);

    public string Id => id;
    public Sprite Sprite => sprite;
    public Color Color => color;
    public Vector2 Size => size;
}
