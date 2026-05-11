using UnityEngine;
using TMPro;

/// <summary>
/// Singleton that tracks the player's wood resource and keeps the HUD counter up to date.
/// This component is created automatically by GameBootstrap, but can also be placed manually
/// on a GameObject in the scene.  Assign <see cref="woodText"/> in the Inspector, or leave it
/// blank and the script will search for a GameObject named "WoodCounter" containing a TMP_Text.
/// </summary>
public class WoodCounter : MonoBehaviour
{
    public static WoodCounter Instance { get; private set; }

    [SerializeField] private TMP_Text woodText;

    private int woodAmount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Auto-locate the TMP text if not assigned in the Inspector
        if (woodText == null)
        {
            var go = GameObject.Find("WoodCounter");
            if (go != null)
                woodText = go.GetComponent<TMP_Text>();
        }

        UpdateDisplay();
    }

    /// <summary>Adds <paramref name="amount"/> units of wood and refreshes the HUD display.</summary>
    public void AddWood(int amount)
    {
        woodAmount += amount;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (woodText != null)
            woodText.text = woodAmount.ToString();
    }
}
