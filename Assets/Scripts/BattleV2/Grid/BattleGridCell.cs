using UnityEngine;

namespace BattleV2.Grid
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BattleGridCell : MonoBehaviour
    {
        [field: SerializeField] public Vector2Int Coordinates { get; private set; }

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Vector2Int coordinates, Sprite sprite, Color tint)
        {
            Coordinates = coordinates;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _spriteRenderer.sprite = sprite;
            _spriteRenderer.color = tint;
        }
    }
}
