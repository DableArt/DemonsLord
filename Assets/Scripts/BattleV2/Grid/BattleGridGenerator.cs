using UnityEngine;

namespace BattleV2.Grid
{
    public class BattleGridGenerator : MonoBehaviour
    {
        [Header("Grid Size")]
        [SerializeField] private int rows = 9;
        [SerializeField] private int columns = 12;
        [SerializeField] private float cellSize = 1f;

        [Header("Visuals")]
        [SerializeField] private BattleGridCell cellPrefab;
        [SerializeField] private Sprite fallbackCellSprite;
        [SerializeField] private Color fallbackCellColor = Color.white;

        private BattleGridCell[,] _cells;
        private Sprite _runtimeFallbackSprite;

        public int Rows => rows;
        public int Columns => columns;
        public float CellSize => cellSize;

        public void Generate(Sprite requestedCellSprite)
        {
            ClearChildren();

            if (rows <= 0 || columns <= 0)
            {
                Debug.LogWarning("[BattleV2] Grid size must be positive.");
                return;
            }

            _cells = new BattleGridCell[columns, rows];
            var spriteToUse = requestedCellSprite != null ? requestedCellSprite : GetFallbackSprite();

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < columns; x++)
                {
                    var cell = CreateCell(x, y);
                    var coordinates = new Vector2Int(x, y);
                    cell.Initialize(coordinates, spriteToUse, fallbackCellColor);
                    _cells[x, y] = cell;
                }
            }
        }

        public Vector3 GetCellCenterWorld(int x, int y)
        {
            var originX = -(columns - 1) * 0.5f * cellSize;
            var originY = -(rows - 1) * 0.5f * cellSize;
            return transform.position + new Vector3(originX + (x * cellSize), originY + (y * cellSize), 0f);
        }

        public Vector3 GetLeftSideDecorPosition(float offsetInCells)
        {
            var centerRow = Mathf.Clamp(rows / 2, 0, rows - 1);
            var leftMostCell = GetCellCenterWorld(0, centerRow);
            return leftMostCell + Vector3.left * (offsetInCells * cellSize);
        }

        private BattleGridCell CreateCell(int x, int y)
        {
            var position = GetCellCenterWorld(x, y);
            BattleGridCell cell;

            if (cellPrefab != null)
            {
                cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                var cellObject = new GameObject($"Cell_{x}_{y}");
                cellObject.transform.SetParent(transform);
                cellObject.transform.position = position;
                var renderer = cellObject.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 0;
                cell = cellObject.AddComponent<BattleGridCell>();
            }

            cell.name = $"Cell_{x}_{y}";
            cell.transform.localScale = Vector3.one * cellSize;
            return cell;
        }

        private Sprite GetFallbackSprite()
        {
            if (fallbackCellSprite != null)
            {
                return fallbackCellSprite;
            }

            if (_runtimeFallbackSprite == null)
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                _runtimeFallbackSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }

            return _runtimeFallbackSprite;
        }

        private void ClearChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(child.gameObject);
                }
                else
#endif
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
