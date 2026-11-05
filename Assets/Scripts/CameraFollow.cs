using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target References")]
    public Transform target;            // Drag your Player GameObject here
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // Pivot point near player's chest/head

    [Header("Orbit Settings")]
    public float distance = 6f;         // Distance from the player
    public float minDistance = 2f;      // Mouse scroll zoom minimum
    public float maxDistance = 12f;     // Mouse scroll zoom maximum
    public float xSpeed = 150f;         // Horizontal rotation speed
    public float ySpeed = 120f;         // Vertical rotation speed

    [Header("Clamping limits")]
    public float yMinLimit = -10f;      // Don't look too far down below player
    public float yMaxLimit = 70f;       // Don't flip completely upside down

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        // Initialize rotations based on current camera placement
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.y;
        yRotation = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Gather Mouse Input for rotation
        xRotation += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
        yRotation -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

        // 2. Clamp vertical rotation so camera doesn't flip upside down
        yRotation = Mathf.Clamp(yRotation, yMinLimit, yMaxLimit);

        // 3. Convert input angles into a rotation quaternion
        Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0f);

        // 4. Handle Zooming with Mouse Scroll Wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scrollInput * 5f, minDistance, maxDistance);

        // 5. Calculate target pivot position and camera placement
        Vector3 targetPivotPosition = target.position + targetOffset;
        Vector3 negativeDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 desiredPosition = (rotation * negativeDistance) + targetPivotPosition;

        // 6. Apply positional transformation and look directly at pivot
        transform.position = desiredPosition;
        transform.rotation = rotation;
    }
}
