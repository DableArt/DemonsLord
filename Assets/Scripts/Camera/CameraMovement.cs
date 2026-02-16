using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    public float lookAheadStrength = 2f;

    private Vector3 lastTargetPos;

    void Start()
    {
        lastTargetPos = target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 currentTargetPos = target.position;


        Vector3 delta = currentTargetPos - lastTargetPos;


        Vector3 lookAhead = delta * lookAheadStrength;


        Vector3 desiredPos = currentTargetPos + offset + lookAhead;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );


        lastTargetPos = currentTargetPos;
    }
}