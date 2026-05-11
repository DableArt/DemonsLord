using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSystem : MonoBehaviour
{
    private const float CardDistanceFromCamera = 1f;

    [SerializeField] private List<GameObject> cardPrefabs = new List<GameObject>();
    [SerializeField] private float summonCooldownTime = 3f;
    [SerializeField] private float animationDuration = 1f;

    private bool _isSummoning;
    private float _nextSummonTime;

    private void OnEnable()
    {
        _nextSummonTime = Time.time + summonCooldownTime;
    }

    private void Update()
    {
        if (Time.time >= _nextSummonTime)
        {
            TriggerSummon();
        }
    }

    public void TriggerSummon()
    {
        if (_isSummoning || Time.time < _nextSummonTime)
        {
            return;
        }

        var prefab = GetRandomCardPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("SummonSystem: No card prefabs assigned for summoning.", this);
            _nextSummonTime = Time.time + summonCooldownTime;
            return;
        }

        StartCoroutine(SummonRoutine(prefab));
        _nextSummonTime = Time.time + summonCooldownTime;
    }

    private GameObject GetRandomCardPrefab()
    {
        GameObject selectedPrefab = null;
        var validPrefabCount = 0;

        for (int i = 0; i < cardPrefabs.Count; i++)
        {
            var prefab = cardPrefabs[i];
            if (prefab == null)
            {
                continue;
            }

            validPrefabCount++;

            if (Random.Range(0, validPrefabCount) == 0)
            {
                selectedPrefab = prefab;
            }
        }

        return selectedPrefab;
    }

    private IEnumerator SummonRoutine(GameObject cardPrefab)
    {
        _isSummoning = true;

        var summonCamera = Camera.main;
        if (summonCamera == null)
        {
            Debug.LogError("SummonSystem: No Main Camera found.", this);
            _isSummoning = false;
            yield break;
        }

        var spawnRotation = Quaternion.Euler(0f, 180f, 0f);
        var spawnedCard = Instantiate(cardPrefab, Vector3.zero, spawnRotation);
        var cameraToCardDepth = summonCamera.nearClipPlane + CardDistanceFromCamera;

        var startPosition = summonCamera.ViewportToWorldPoint(new Vector3(0.5f, -0.2f, cameraToCardDepth));
        var targetPosition = summonCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cameraToCardDepth));

        spawnedCard.transform.position = startPosition;

        var targetRotation = Quaternion.Euler(0f, 0f, 0f);
        var elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            var t = animationDuration > 0f ? Mathf.Clamp01(elapsed / animationDuration) : 1f;

            spawnedCard.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            spawnedCard.transform.rotation = Quaternion.Lerp(spawnRotation, targetRotation, t);

            yield return null;
        }

        Debug.Log("добавлен новый герой");

        yield return new WaitForSeconds(1.5f);

        if (spawnedCard != null)
        {
            Destroy(spawnedCard);
        }

        _isSummoning = false;
    }
}
