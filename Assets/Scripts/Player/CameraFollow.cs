using UnityEngine;

[ExecuteInEditMode]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;
    [SerializeField] float smoothTime = 0.3f;
    Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        if (Application.isPlaying)
            UpdateGame();
        else
            UpdateEditor();
    }

    void UpdateEditor()
    {
        Vector3 desiredPosition = CalculatePositionBasedOnOffset();
        transform.position = desiredPosition;
        transform.LookAt(target.position);
    }

    void UpdateGame()
    {
        Vector3 desiredPosition = CalculatePositionBasedOnOffset();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(target.position);
    }

    Vector3 CalculatePositionBasedOnOffset()
    {
        Vector3 forward = target.rotation * Vector3.forward;
        Vector3 right = target.rotation * Vector3.right;
        Vector3 up = target.rotation * Vector3.up;

        Vector3 desiredPosition = target.position
            + forward * offset.z
            + right * offset.x
            + up * offset.y;

        return desiredPosition;
    }
}
