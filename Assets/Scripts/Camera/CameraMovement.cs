using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    ​public Transform target;
    public float smoothSpeed = 5f,
    overSpeed; public Vector3 offset, oldPos;
    private void LateUpdate()
    {
        ​ Vector3 delta = oldPos - target.position, desiredPos = target.position + offset + (delta * overSpeed);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime); 
        oldPos = target.position;
    }
    ​
}
