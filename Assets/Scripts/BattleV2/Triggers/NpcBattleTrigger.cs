using BattleV2.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleV2.Triggers
{
    public interface IBattleTileVisualProvider
    {
        Sprite ResolveGroundSprite(Vector3 worldPosition);
        string ResolveGroundVisualId(Vector3 worldPosition);
    }

    [RequireComponent(typeof(Collider2D))]
    public class NpcBattleTrigger : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string battleSceneName = "BattleSceneV2";

        [Header("Battle Data")]
        [SerializeField] private string enemyId = "enemy";
        [SerializeField] private Vector2Int playerSpawn = new Vector2Int(1, 4);
        [SerializeField] private Vector2Int enemySpawn = new Vector2Int(10, 4);

        [Header("Player Detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Tile Visual Integration")]
        [SerializeField] private MonoBehaviour tileVisualProviderSource;
        [SerializeField] private Sprite fallbackGroundSprite;
        [SerializeField] private string fallbackGroundVisualId = "default";

        private IBattleTileVisualProvider _tileVisualProvider;
        private bool _isTriggered;

        private void Awake()
        {
            _tileVisualProvider = tileVisualProviderSource as IBattleTileVisualProvider;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryEnterBattle(other != null ? other.gameObject : null);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryEnterBattle(other != null ? other.gameObject : null);
        }

        private void TryEnterBattle(GameObject other)
        {
            if (_isTriggered || other == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            {
                return;
            }

            _isTriggered = true;

            var playerPosition = other.transform.position;
            var data = new BattleEntryData
            {
                BattleSceneName = battleSceneName,
                EnemyId = enemyId,
                PlayerWorldPosition = playerPosition,
                PlayerGridSpawn = playerSpawn,
                EnemyGridSpawn = enemySpawn,
                GroundSprite = ResolveGroundSprite(playerPosition),
                GroundVisualId = ResolveGroundVisualId(playerPosition)
            };

            BattleEntryStorage.Set(data);

            if (string.IsNullOrWhiteSpace(battleSceneName))
            {
                Debug.LogError("[BattleV2] Battle scene name is empty.");
                _isTriggered = false;
                return;
            }

            SceneManager.LoadScene(battleSceneName);
        }

        private Sprite ResolveGroundSprite(Vector3 worldPosition)
        {
            if (_tileVisualProvider != null)
            {
                var sprite = _tileVisualProvider.ResolveGroundSprite(worldPosition);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return fallbackGroundSprite;
        }

        private string ResolveGroundVisualId(Vector3 worldPosition)
        {
            if (_tileVisualProvider != null)
            {
                var visualId = _tileVisualProvider.ResolveGroundVisualId(worldPosition);
                if (!string.IsNullOrWhiteSpace(visualId))
                {
                    return visualId;
                }
            }

            return fallbackGroundVisualId;
        }
    }
}
