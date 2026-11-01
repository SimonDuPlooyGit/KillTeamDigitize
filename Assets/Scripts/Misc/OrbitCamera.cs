using UnityEngine;

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

    // State Tracking
    public bool isFreecam = false;
    private float mouseX = 0f;
    private float mouseY = 0f;
    public bool switchOffForMenu = false;

    private void Start()
    {
        UpdateCursorLock();
    }
    
    private void Update()
    {
        // Toggle between Orbit and Freecam when the toggle key is pressed
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetFreecam(!isFreecam);
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

    private void SetFreecam(bool freecamState)
    {
        isFreecam = freecamState;
        UpdateCursorLock();
        
        if (!isFreecam && lookAtTarget != null)
        {
            transform.position = lookAtTarget.position - transform.forward * orbitRadius;
        }
    }

    private void UpdateCursorLock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    //Logic to use in update to handle orbiting movement
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

    //Logic to use in update for freecam movement
    private void HandleFreecamMovement()
    {
        if (!switchOffForMenu)
        {
            if (Input.GetMouseButton(0))
            {
                //Mouse Look Rotation
                float mouseInputX = Input.GetAxis("Mouse X") * sensitivity;
                float mouseInputY = Input.GetAxis("Mouse Y") * sensitivity;
        
                transform.Rotate(-mouseInputY, mouseInputX, 0, Space.Self);
        
                //Lock roll to prevent camera tilt
                Vector3 eulerRotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
            }
            
            //Keyboard Movement (WASD / Arrows)
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 moveInput = new Vector3(moveX, 0f, moveZ);
        
            transform.Translate(moveInput * speed * Time.deltaTime, Space.Self);

            float moveY = 0f;
            if (Input.GetKey(KeyCode.Space)) moveY += 0.25f;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) moveY -= 0.25f;
            
            transform.position += Vector3.up * moveY * speed * Time.deltaTime;
        }
    }

    //Taggle between freecam and orbit cam
    public void ToggleCam()
    {
        SetFreecam(!isFreecam);
    }

    //Set to orbit cam and set a unit as the lookat target
    public void SetOrbitTarget(Transform target)
    {
        lookAtTarget = target;
        SetFreecam(false);
    }
}