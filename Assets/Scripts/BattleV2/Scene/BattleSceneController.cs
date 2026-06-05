using System;
using BattleV2.Core;
using BattleV2.Data;
using BattleV2.Grid;
using BattleV2.Units;
using UnityEngine;

namespace BattleV2.Scene
{
    public class BattleSceneController : MonoBehaviour
    {
        [Header("Scene Links")]
        [SerializeField] private BattleGridGenerator gridGenerator;

        [Header("Spawned Objects")]
        [SerializeField] private GameObject leftDecorativePlayerPrefab;
        [SerializeField] private BattleUnit playerUnitPrefab;
        [SerializeField] private BattleUnit enemyUnitPrefab;

        [Header("Fallback visuals")]
        [SerializeField] private Sprite fallbackUnitSprite;
        [SerializeField] private Color playerColor = new Color(0.35f, 0.75f, 1f, 1f);
        [SerializeField] private Color enemyColor = new Color(1f, 0.45f, 0.45f, 1f);

        [Header("Layout")]
        [SerializeField] private float leftDecorOffsetInCells = 1.5f;

        private BattleState _state = BattleState.None;

        public event Action<BattleState> StateChanged;

        public void Initialize(BattleEntryData entryData)
        {
            SetState(BattleState.Initializing);

            if (gridGenerator == null)
            {
                Debug.LogError("[BattleV2] BattleGridGenerator reference is missing.");
                SetState(BattleState.Completed);
                return;
            }

            gridGenerator.Generate(entryData != null ? entryData.GroundSprite : null);

            SpawnLeftDecorativePlayer();
            SpawnPlayerUnit(entryData);
            SpawnEnemyUnit(entryData);

            SetState(BattleState.WaitingForInput);
        }

        private void SpawnLeftDecorativePlayer()
        {
            var position = gridGenerator.GetLeftSideDecorPosition(leftDecorOffsetInCells);

            if (leftDecorativePlayerPrefab != null)
            {
                Instantiate(leftDecorativePlayerPrefab, position, Quaternion.identity, transform);
                return;
            }

            CreateFallbackVisual("PlayerDecor", position, playerColor, sortingOrder: 2);
        }

        private void SpawnPlayerUnit(BattleEntryData entryData)
        {
            var spawn = entryData != null ? entryData.PlayerGridSpawn : new Vector2Int(1, 4);
            spawn = ClampToGrid(spawn);
            var position = gridGenerator.GetCellCenterWorld(spawn.x, spawn.y);

            if (playerUnitPrefab != null)
            {
                var unit = Instantiate(playerUnitPrefab, position, Quaternion.identity, transform);
                unit.Initialize(BattleUnitSide.Player, "player");
                return;
            }

            CreateFallbackUnit("PlayerUnit", position, BattleUnitSide.Player, "player", playerColor);
        }

        private void SpawnEnemyUnit(BattleEntryData entryData)
        {
            var spawn = entryData != null ? entryData.EnemyGridSpawn : new Vector2Int(10, 4);
            spawn = ClampToGrid(spawn);
            var position = gridGenerator.GetCellCenterWorld(spawn.x, spawn.y);
            var enemyId = entryData != null ? entryData.EnemyId : string.Empty;

            if (enemyUnitPrefab != null)
            {
                var unit = Instantiate(enemyUnitPrefab, position, Quaternion.identity, transform);
                unit.Initialize(BattleUnitSide.Enemy, enemyId);
                return;
            }

            CreateFallbackUnit("EnemyUnit", position, BattleUnitSide.Enemy, enemyId, enemyColor);
        }

        private void CreateFallbackUnit(string objectName, Vector3 position, BattleUnitSide side, string unitId, Color color)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform);
            go.transform.position = position;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = fallbackUnitSprite != null ? fallbackUnitSprite : RuntimeSprites.GetWhiteSprite();
            renderer.color = color;
            renderer.sortingOrder = 3;

            var unit = go.AddComponent<BattleUnit>();
            unit.Initialize(side, unitId);
        }

        private void CreateFallbackVisual(string objectName, Vector3 position, Color color, int sortingOrder)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform);
            go.transform.position = position;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = fallbackUnitSprite != null ? fallbackUnitSprite : RuntimeSprites.GetWhiteSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private Vector2Int ClampToGrid(Vector2Int value)
        {
            var x = Mathf.Clamp(value.x, 0, gridGenerator.Columns - 1);
            var y = Mathf.Clamp(value.y, 0, gridGenerator.Rows - 1);
            return new Vector2Int(x, y);
        }

        private void SetState(BattleState state)
        {
            _state = state;
            StateChanged?.Invoke(_state);
        }

        private static class RuntimeSprites
        {
            private static Sprite _whiteSprite;

            public static Sprite GetWhiteSprite()
            {
                if (_whiteSprite == null)
                {
                    var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    _whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                }

                return _whiteSprite;
            }
        }
    }
}
