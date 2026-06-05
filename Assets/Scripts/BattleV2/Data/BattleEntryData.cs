using UnityEngine;

namespace BattleV2.Data
{
    public sealed class BattleEntryData
    {
        public string BattleSceneName;
        public string EnemyId;
        public string GroundVisualId;
        public Sprite GroundSprite;
        public Vector3 PlayerWorldPosition;
        public Vector2Int PlayerGridSpawn;
        public Vector2Int EnemyGridSpawn;

        public BattleEntryData()
        {
            BattleSceneName = string.Empty;
            EnemyId = string.Empty;
            GroundVisualId = string.Empty;
            GroundSprite = null;
            PlayerWorldPosition = Vector3.zero;
            PlayerGridSpawn = new Vector2Int(1, 4);
            EnemyGridSpawn = new Vector2Int(10, 4);
        }
    }

    public static class BattleEntryStorage
    {
        private static BattleEntryData _current;

        public static bool HasData => _current != null;

        public static BattleEntryData Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new BattleEntryData();
                }

                return _current;
            }
        }

        public static void Set(BattleEntryData data)
        {
            _current = data ?? new BattleEntryData();
        }

        public static BattleEntryData ConsumeOrDefault()
        {
            var data = _current ?? new BattleEntryData();
            _current = null;
            return data;
        }

        public static void Clear()
        {
            _current = null;
        }
    }
}
