using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridObjectBehaviour))]
public class GridObjectBehaviourEditor : Editor
{
    private static GridEditorSettings settings;
    private const string settingsAssetName = "GridEditorSettings";
    private static GridBehaviour lastGrid = null;
    private static int lastFrame = -1;
    private static Dictionary<(Vector2Int, int), GridObjectBehaviour> gridMapCache = null;

    private Vector3 _prevWorldPosition;
    private Vector2Int _prevGridPosition;

    [NonSerialized]
    private GridBehaviour _cachedGrid;
    [NonSerialized]
    private Transform _cachedTransform;

    private GridBehaviour FindGridInParents(Transform start)
    {
        while (start != null)
        {
            var grid = start.GetComponent<GridBehaviour>();
            if (grid != null)
                return grid;
            start = start.parent;
        }
        return null;
    }

    private GridBehaviour GetCachedGrid(GridObjectBehaviour gridObj)
    {
        if (_cachedGrid == null || _cachedTransform != gridObj.transform)
        {
            _cachedGrid = FindGridInParents(gridObj.transform);
            _cachedTransform = gridObj.transform;
        }
        return _cachedGrid;
    }



    // --- Settings loader ---
    private void EnsureSettings()
    {
        if (settings != null) return;
        settings = Resources.Load<GridEditorSettings>(settingsAssetName);
#if UNITY_EDITOR
        if (settings == null)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(GridEditorSettings)}");
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<GridEditorSettings>(assetPath);
            }
        }
#endif
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<GridEditorSettings>();
        }
    }

    // --- Cache map for this frame and grid ---
    private static Dictionary<(Vector2Int, int), GridObjectBehaviour> GetGridMap(GridBehaviour grid)
    {
        int currentFrame = Time.frameCount; // можно использовать EditorApplication.timeSinceStartup дл€ редактора
        if (lastGrid != grid || lastFrame != currentFrame || gridMapCache == null)
        {
            lastGrid = grid;
            lastFrame = currentFrame;
            gridMapCache = grid != null ? grid.CreateGridObjectMap() : null;
        }
        return gridMapCache;
    }

    private void OnEnable()
    {
        var gridObj = (GridObjectBehaviour)target;
        _prevWorldPosition = gridObj.transform.position;
        _prevGridPosition = gridObj.GridPosition;
        EnsureSettings();
    }

    public override void OnInspectorGUI()
    {
        var gridObj = (GridObjectBehaviour)target;
        DrawDefaultInspector();
        var grid = GetCachedGrid(gridObj);
        if (grid != null) SnapTransformToGridIfNeeded(gridObj, grid);
    }

    private void OnSceneGUI()
    {
        EnsureSettings();
        var gridObj = (GridObjectBehaviour)target;
        var grid = GetCachedGrid(gridObj);
        if (grid == null) return;

        SnapTransformToGridIfNeeded(gridObj, grid);
        DrawFadingGrid(gridObj, grid);
        DrawHighlightCell(gridObj, grid);
    }


    private void SnapTransformToGridIfNeeded(GridObjectBehaviour gridObj, GridBehaviour grid)

    {
        if (grid == null) return;

        Vector2Int oldGridPos = gridObj.GridPosition;

        // 1. ¬ычислить желаемую клетку (GridPosition)
        Vector2Int targetGridPos = gridObj.GridPosition;
        if (gridObj.transform.position != _prevWorldPosition)
            targetGridPos = grid.WorldToCell(gridObj.transform.position);

        // 2. ѕровер€ем, можно ли разместить
        var map = GetGridMap(grid);
        if (!CanPlace(gridObj, targetGridPos, map))
            targetGridPos = FindNearestFreeCell(targetGridPos, gridObj.Layer, grid, gridObj, map);

        // 3. ћен€ем GridPosition и transform только если надо
        if (gridObj.GridPosition != targetGridPos)
            gridObj.GridPosition = targetGridPos;

        Vector3 cellWorld = grid.GetCellCenterWorld(targetGridPos);
        if ((Vector2)gridObj.transform.position != (Vector2)cellWorld)
        {
            gridObj.transform.position = cellWorld;
            EditorUtility.SetDirty(gridObj.transform);
        }

        _prevWorldPosition = gridObj.transform.position;
        _prevGridPosition = gridObj.GridPosition;
    }

    private Vector2Int FindNearestFreeCell(
        Vector2Int targetCell,
        int layer,
        GridBehaviour grid,
        GridObjectBehaviour self,
        Dictionary<(Vector2Int, int), GridObjectBehaviour> map)
    {
        if (!map.TryGetValue((targetCell, layer), out var obj) || obj == self)
            return targetCell;

        int maxRadius = settings.gridDrawRadius;
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int dy = radius - Mathf.Abs(dx);
                foreach (int sign in new int[] { -1, 1 })
                {
                    foreach (Vector2Int offset in new Vector2Int[] {
                        new Vector2Int(dx, sign * dy),
                        new Vector2Int(sign * dy, dx)
                    })
                    {
                        Vector2Int checkCell = targetCell + offset;
                        var key = (checkCell, layer);
                        if (!map.ContainsKey(key))
                            return checkCell;
                    }
                }
            }
        }
        return targetCell;
    }

    private bool CanPlace(GridObjectBehaviour gridObj, Vector2Int cell, Dictionary<(Vector2Int, int), GridObjectBehaviour> map)
    {
        if (map == null) return true;
        var key = (cell, gridObj.Layer);
        if (map.TryGetValue(key, out var foundObj))
            return foundObj == gridObj;
        return true;
    }

    private void DrawFadingGrid(GridObjectBehaviour gridObj, GridBehaviour grid)
    {
        EnsureSettings();
        var map = GetGridMap(grid);
        var cellSize = grid.tilemap.cellSize;
        Vector2Int centerCell = gridObj.GridPosition;
        int radiusSqr = settings.gridDrawRadius * settings.gridDrawRadius;

        for (int dx = -settings.gridDrawRadius; dx <= settings.gridDrawRadius; dx++)
        {
            for (int dy = -settings.gridDrawRadius; dy <= settings.gridDrawRadius; dy++)
            {
                Vector2Int curCell = centerCell + new Vector2Int(dx, dy);
                int dist2 = dx * dx + dy * dy;
                if (dist2 > radiusSqr)
                    continue;

                float distance = Mathf.Sqrt(dist2);
                float t = Mathf.Clamp01(1f - distance / settings.gridDrawRadius);
                float alpha = Mathf.Lerp(settings.minAlpha, settings.maxAlpha, t);

                if (alpha <= 0f)
                    continue;

                bool canPlace = CanPlace(gridObj, curCell, map);
                var color = canPlace ? settings.gridColor : settings.blockedGridColor;
                color.a = alpha;
                Handles.color = color;

                Vector3 cellCenter = grid.GetCellCenterWorld(curCell);
                Vector3 halfSize = (Vector3)cellSize / 2f;
                Vector3 p0 = cellCenter + new Vector3(-halfSize.x, -halfSize.y, 0);
                Vector3 p1 = cellCenter + new Vector3(halfSize.x, -halfSize.y, 0);
                Vector3 p2 = cellCenter + new Vector3(halfSize.x, halfSize.y, 0);
                Vector3 p3 = cellCenter + new Vector3(-halfSize.x, halfSize.y, 0);

                Handles.DrawAAPolyLine(settings.gridLineWidth, p0, p1);
                Handles.DrawAAPolyLine(settings.gridLineWidth, p1, p2);
                Handles.DrawAAPolyLine(settings.gridLineWidth, p2, p3);
                Handles.DrawAAPolyLine(settings.gridLineWidth, p3, p0);
            }
        }
    }

    private void DrawHighlightCell(GridObjectBehaviour gridObj, GridBehaviour grid)
    {
        EnsureSettings();
        var map = GetGridMap(grid);

        var cellSize = grid.tilemap.cellSize;
        Vector2Int cell = gridObj.GridPosition;
        Vector3 cellCenter = grid.GetCellCenterWorld(cell);
        Vector3 hSize = (Vector3)cellSize / 2f;
        Vector3[] verts = new Vector3[] {
            cellCenter + new Vector3(-hSize.x, -hSize.y, 0),
            cellCenter + new Vector3( hSize.x, -hSize.y, 0),
            cellCenter + new Vector3( hSize.x,  hSize.y, 0),
            cellCenter + new Vector3(-hSize.x,  hSize.y, 0)
        };

        bool canPlace = CanPlace(gridObj, cell, map);

        Handles.color = settings.highlightFill;
        Handles.DrawAAConvexPolygon(verts);

        Handles.color = canPlace ? settings.highlightOutline : settings.blockedHighlightOutline;
        Handles.DrawAAPolyLine(settings.highlightOutlineWidth, verts[0], verts[1]);
        Handles.DrawAAPolyLine(settings.highlightOutlineWidth, verts[1], verts[2]);
        Handles.DrawAAPolyLine(settings.highlightOutlineWidth, verts[2], verts[3]);
        Handles.DrawAAPolyLine(settings.highlightOutlineWidth, verts[3], verts[0]);
    }
}
