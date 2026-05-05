using UnityEngine;

public class NpcAi : MonoBehaviour
{
    private enum State
    {
        Idle,
        Walk
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Idle Time")]
    [SerializeField] private float idleTimeMin = 1f;
    [SerializeField] private float idleTimeMax = 3f;

    [Header("Walk Area")]
    [SerializeField] private Transform areaCenter;
    [SerializeField] private Vector2 areaSize = new Vector2(5f, 5f);

    [Header("Links")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private State currentState;
    private float stateTimer;
    private Vector3 targetPosition;

    private void Start()
    {
        SetIdle();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;

            case State.Walk:
                UpdateWalk();
                break;
        }
    }

    private void UpdateIdle()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            PickRandomTarget();
            SetWalk();
        }
    }

    private void UpdateWalk()
    {
        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, targetPosition, moveSpeed * Time.deltaTime);
        transform.position = next;

        Vector3 dir = targetPosition - transform.position;

        if (dir.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (dir.x < -0.01f)
            spriteRenderer.flipX = true;

        if (Vector3.Distance(transform.position, targetPosition) <= arriveDistance)
        {
            SetIdle();
        }
    }

    private void SetIdle()
    {
        currentState = State.Idle;
        stateTimer = Random.Range(idleTimeMin, idleTimeMax);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void SetWalk()
    {
        currentState = State.Walk;

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }
    }

    private void PickRandomTarget()
    {
        Vector3 center = areaCenter != null ? areaCenter.position : transform.position;

        float randomX = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        float randomY = Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);

        targetPosition = new Vector3(
            center.x + randomX,
            center.y + randomY,
            transform.position.z
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = areaCenter != null ? areaCenter.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, areaSize.y, 0f));
    }
}
