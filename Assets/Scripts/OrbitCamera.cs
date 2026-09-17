using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("Orbit Settings")]
    [SerializeField] private float maximumDistance = 5f;
    [SerializeField] private float minimumDistance = 1f;
    private float orbitRadius = 3f;

    [Header("Freecam Settings")]
    [SerializeField] private float speed = 10f;

    // State Tracking
    private bool isFreecam = false;
    private float mouseX = 0f;
    private float mouseY = 0f;

    void Update()
    {
        // Toggle between Orbit and Freecam when the toggle key is pressed
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isFreecam = !isFreecam;
            
            // Reset orbit radius or position smoothly if needed when switching back
            if (!isFreecam && lookAtTarget != null)
            {
                transform.position = lookAtTarget.position - transform.forward * orbitRadius;
            }
        }

        // Execute behavior based on current mode
        if (isFreecam)
        {
            HandleFreecamMovement();
        }
        else
        {
            HandleOrbitMovement();
        }
    }

    private void HandleOrbitMovement()
    {
        if (lookAtTarget == null) return;

        // Orbit rotation using Left Mouse Button
        if (Input.GetMouseButton(0))
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
            transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
        }

        // Zoom with scroll wheel
        orbitRadius -= Input.mouseScrollDelta.y * (sensitivity * 0.1f);
        orbitRadius = Mathf.Clamp(orbitRadius, minimumDistance, maximumDistance);

        // Position camera relative to target
        transform.position = lookAtTarget.position - transform.forward * orbitRadius;
    }

    private void HandleFreecamMovement()
    {
        // Mouse Look Rotation
        float mouseInputX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseInputY = Input.GetAxis("Mouse Y") * sensitivity;
        
        transform.Rotate(-mouseInputY, mouseInputX, 0, Space.Self);
        
        // Lock roll to prevent camera tilt
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);

        // Keyboard Movement (WASD / Arrows)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveInput = new Vector3(moveX, 0f, moveZ);
        
        transform.Translate(moveInput * speed * Time.deltaTime, Space.Self);
    }
}