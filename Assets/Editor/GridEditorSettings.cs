using UnityEngine;

[CreateAssetMenu(menuName = "Tools/Grid Editor Settings")]
public class GridEditorSettings : ScriptableObject
{
    [Header("Grid Fade Radius")]
    public int gridDrawRadius = 5;

    [Header("Fade Alpha (center/max)")]
    [Range(0.01f, 1f)]
    public float maxAlpha = 0.5f;

    [Header("Fade Alpha (edge/min)")]
    [Range(0f, 1f)]
    public float minAlpha = 0f;

    [Header("Grid Line Width")]
    [Range(0f, 100f)]
    public float gridLineWidth = 2f;

    [Header("Highlight Outline Width")]
    [Range(0f, 100f)]
    public float highlightOutlineWidth = 4f;

    [Header("Grid Color")]
    public Color gridColor = Color.white;

    [Header("Grid Color (Blocked)")]
    public Color blockedGridColor = Color.red;

    [Header("Highlight Fill Color")]
    public Color highlightFill = new Color(1f, 1f, 0.2f, 0.33f);

    [Header("Highlight Outline Color")]
    public Color highlightOutline = Color.yellow;

    [Header("Highlight Outline Color (Blocked)")]
    public Color blockedHighlightOutline = Color.red;
}
