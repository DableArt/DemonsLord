using TMPro;
using UnityEngine;

namespace WorldGenerate
{
    /// <summary>
    /// Handles the tree-chopping mini-game:
    ///  • Detects when the Player enters interaction range.
    ///  • Shows a "F" TextMeshPro label above the tree.
    ///  • While the Player holds F, a QTE progress bar fills beneath the prompt.
    ///  • When the bar is full the tree is destroyed and <see cref="WoodCounter"/> receives
    ///    <see cref="woodAmount"/> units of wood.
    ///  • Releasing F before completion, or walking away, resets the bar.
    ///
    /// Attach this component to the Tree prefab.
    /// The Player GameObject must have the tag "Player".
    /// </summary>
    public class TreeInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private float chopDuration = 2f;
        [SerializeField] private int woodAmount = 10;

        [Header("UI Layout")]
        [SerializeField] private float promptYOffset = 1.5f;
        [SerializeField] private float barYOffset = 1.0f;
        [SerializeField] private float barWidth = 1.0f;
        [SerializeField] private float barHeight = 0.12f;

        // Shared white sprite used by all tree instances to avoid per-instance allocations
        private static Sprite _sharedWhiteSprite;

        // Runtime state
        private bool playerInRange;
        private float chopProgress;

        // UI references (built at runtime)
        private TextMeshPro promptText;
        private Transform progressBGTransform;
        private Transform progressFillTransform;

        // -------------------------------------------------------------------------
        private void Awake()
        {
            // Proximity trigger — kept separate from the tree's solid BoxCollider2D
            var trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.radius = interactionRadius;
            trigger.isTrigger = true;

            BuildUI();
            SetUIVisible(false);
        }

        private static Sprite GetOrCreateWhiteSprite()
        {
            if (_sharedWhiteSprite == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _sharedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return _sharedWhiteSprite;
        }

        // -------------------------------------------------------------------------
        // Build world-space TMP label + SpriteRenderer progress bars at runtime
        // -------------------------------------------------------------------------
        private void BuildUI()
        {
            var whiteSprite = GetOrCreateWhiteSprite();

            // --- "F" prompt (TextMeshPro 3-D) ---
            var promptGO = new GameObject("TreePrompt");
            promptGO.transform.SetParent(transform);
            promptGO.transform.localPosition = new Vector3(0f, promptYOffset, -0.1f);

            promptText = promptGO.AddComponent<TextMeshPro>();
            promptText.text = "F";
            promptText.fontSize = 4f;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.yellow;
            promptText.sortingOrder = 10;

            // --- Background bar ---
            var bgGO = new GameObject("ProgressBG");
            bgGO.transform.SetParent(transform);
            bgGO.transform.localPosition = new Vector3(0f, barYOffset, -0.1f);
            bgGO.transform.localScale = new Vector3(barWidth, barHeight, 1f);

            var bgSR = bgGO.AddComponent<SpriteRenderer>();
            bgSR.sprite = whiteSprite;
            bgSR.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            bgSR.sortingOrder = 9;

            progressBGTransform = bgGO.transform;

            // --- Fill bar ---
            var fillGO = new GameObject("ProgressFill");
            fillGO.transform.SetParent(transform);
            // Anchor left — starts invisible (width = 0)
            fillGO.transform.localPosition = new Vector3(-barWidth * 0.5f, barYOffset, -0.15f);
            fillGO.transform.localScale = new Vector3(0f, barHeight, 1f);

            var fillSR = fillGO.AddComponent<SpriteRenderer>();
            fillSR.sprite = whiteSprite;
            fillSR.color = new Color(0.2f, 0.85f, 0.2f, 0.9f);
            fillSR.sortingOrder = 10;

            progressFillTransform = fillGO.transform;
        }

        // -------------------------------------------------------------------------
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = true;

            // Show prompt only; the bar appears once F is pressed
            promptText.gameObject.SetActive(true);
            progressBGTransform.gameObject.SetActive(false);
            progressFillTransform.gameObject.SetActive(false);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = false;
            chopProgress = 0f;
            SetUIVisible(false);
        }

        // -------------------------------------------------------------------------
        private void Update()
        {
            if (!playerInRange) return;

            if (Input.GetKey(KeyCode.F))
            {
                // Show the progress bar on first frame of pressing F
                if (!progressBGTransform.gameObject.activeSelf)
                {
                    progressBGTransform.gameObject.SetActive(true);
                    progressFillTransform.gameObject.SetActive(true);
                }

                chopProgress += Time.deltaTime / chopDuration;
                chopProgress = Mathf.Clamp01(chopProgress);

                UpdateFillBar(chopProgress);

                if (chopProgress >= 1f)
                    FinishChopping();
            }
            else if (chopProgress > 0f)
            {
                // Key released before completion — reset progress
                chopProgress = 0f;
                progressBGTransform.gameObject.SetActive(false);
                progressFillTransform.gameObject.SetActive(false);
            }
        }

        // -------------------------------------------------------------------------
        private void UpdateFillBar(float t)
        {
            float fillW = t * barWidth;
            // Keep fill anchored to the left edge of the background bar
            progressFillTransform.localScale = new Vector3(fillW, barHeight, 1f);
            progressFillTransform.localPosition = new Vector3(-barWidth * 0.5f + fillW * 0.5f, barYOffset, -0.15f);
        }

        private void FinishChopping()
        {
            WoodCounter.Instance?.AddWood(woodAmount);
            Destroy(gameObject);
        }

        private void SetUIVisible(bool visible)
        {
            if (promptText != null)
                promptText.gameObject.SetActive(visible);
            if (progressBGTransform != null)
                progressBGTransform.gameObject.SetActive(visible);
            if (progressFillTransform != null)
                progressFillTransform.gameObject.SetActive(visible);
        }
    }
}
