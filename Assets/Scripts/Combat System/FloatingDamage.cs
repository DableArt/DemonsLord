using UnityEngine;
using TMPro;

public class FloatingDamage : MonoBehaviour
{
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Canvas worldCanvas;

    private static FloatingDamage _instance;

    private void Awake()
    {
        _instance = this;
    }

    public static void ShowDamage(Unit target, int amount, bool isCrit = false)
    {
        if (_instance == null || target == null) return;
        Color color = isCrit ? new Color(1f, 0.84f, 0f) : new Color(1f, 0.27f, 0.27f);
        Spawn(target.transform.position + Vector3.up * 0.3f, amount.ToString(), color, isCrit ? 6 : 4);
    }

    public static void ShowHeal(Unit target, int amount)
    {
        if (_instance == null || target == null) return;
        Spawn(target.transform.position + Vector3.up * 0.3f, $"+{amount}", new Color(0.27f, 1f, 0.27f), 4);
    }

    public static void ShowUltimate(Unit target, int amount)
    {
        if (_instance == null || target == null) return;
        Spawn(target.transform.position + Vector3.up * 0.5f, amount.ToString(), new Color(0.67f, 0.27f, 1f), 6);
    }

    public static void ShowText(Unit target, string text, Color color, int fontSize = 4)
    {
        if (_instance == null || target == null) return;
        Spawn(target.transform.position + Vector3.up * 0.3f, text, color, fontSize);
    }

    private static void Spawn(Vector3 position, string text, Color color, int fontSize)
    {
        if (_instance.floatingTextPrefab != null)
        {
            var go = Instantiate(_instance.floatingTextPrefab, position, Quaternion.identity);
            var tmp = go.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.color = color;
                tmp.fontSize = fontSize;
            }
            go.AddComponent<FloatingTextAnimator>();
        }
        else
        {
            var go = new GameObject("FloatingText");
            go.transform.position = position;
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            go.AddComponent<FloatingTextAnimator>();
        }
    }
}

public class FloatingTextAnimator : MonoBehaviour
{
    private TextMeshPro _text;
    private float _duration = 1.2f;
    private float _elapsed;
    private Vector3 _startPos;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        if (_text == null)
            _text = GetComponentInChildren<TextMeshPro>();
        _startPos = transform.position;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        transform.position = _startPos + Vector3.up * (t * 2.5f);

        if (_text != null)
        {
            var c = _text.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            _text.color = c;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}
