using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbitCamera : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] public Transform lookAtTarget;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("Orbit Settings")]
    [SerializeField] private float maximumDistance = 5f;
    [SerializeField] private float minimumDistance = 1f;
    private float orbitRadius = 3f;

    [Header("Freecam Settings")]
    [SerializeField] private float speed = 10f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI camText;

    // State Tracking
    public bool isFreecam = false;
    private float mouseX = 0f;
    private float mouseY = 0f;
    public bool switchOffForMenu = false;
    public bool paused = false;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Ensure rotation is locked programmatically as well
        UpdateCursorLock();
    }
    
    private void Update()
    {
        // Toggle between Orbit and Freecam when the toggle key is pressed
        if (Input.GetKeyDown(toggleKey) && !paused)
        {
            SetFreecam(!isFreecam);
        }

        // Execute behavior based on current mode
        if (isFreecam && !paused)
        {
            HandleFreecamMovement();
        }
        else if (!isFreecam && !paused)
        {
            HandleOrbitMovement();
        }

        camText.text = isFreecam ? "Free cam" : "Orbit cam";
    }

    private void SetFreecam(bool freecamState)
    {
        isFreecam = freecamState;

        if (isFreecam)
        {
            rb.isKinematic = !isFreecam;
        }
        
        UpdateCursorLock();
        
        if (!isFreecam && lookAtTarget != null)
        {
            Vector3 targetPos = lookAtTarget.position - transform.forward * orbitRadius;
            if (rb != null) rb.MovePosition(targetPos);
            else transform.position = targetPos;
        }
    }

    private void UpdateCursorLock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    // Logic to use in update to handle orbiting movement
    private void HandleOrbitMovement()
    {
        if (!switchOffForMenu)
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
    }

    // Logic to use in update for freecam movement
    private void HandleFreecamMovement()
    {
        if (!switchOffForMenu)
        {
            if (Input.GetMouseButton(0))
            {
                // Mouse Look Rotation
                float mouseInputX = Input.GetAxis("Mouse X") * sensitivity;
                float mouseInputY = Input.GetAxis("Mouse Y") * sensitivity;
        
                transform.Rotate(-mouseInputY, mouseInputX, 0, Space.Self);
        
                // Lock roll to prevent camera tilt
                Vector3 eulerRotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
            }
            
            // Keyboard Movement (WASD / Arrows)
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 moveInput = new Vector3(moveX, 0f, moveZ);
        
            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }
            
            // Convert local movement vector to world space based on camera rotation
            Vector3 worldMove = transform.TransformDirection(moveInput) * speed * Time.deltaTime;

            float moveY = 0f;
            if (Input.GetKey(KeyCode.Space)) moveY += 0.25f;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) moveY -= 0.25f;
            
            worldMove += Vector3.up * moveY * speed * Time.deltaTime;

            // Move via Rigidbody physics to respect colliders without force jitter
            if (rb != null)
            {
                rb.MovePosition(rb.position + worldMove);
            }
            else
            {
                transform.position += worldMove;
            }
        }
    }

    // Toggle between freecam and orbit cam
    public void ToggleCam()
    {
        SetFreecam(!isFreecam);

        if(isFreecam)
        {
            camText.text = "Free cam";
        }
        else
        {
            camText.text = "Orbit cam";
        }
    }

    // Set to orbit cam and set a unit as the lookat target
    public void SetOrbitTarget(Transform target)
    {
        lookAtTarget = target;
        SetFreecam(false);
    }
}