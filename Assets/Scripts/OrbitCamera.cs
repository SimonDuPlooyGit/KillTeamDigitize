using UnityEngine;
using UnityEngine.InputSystem;
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float maximumDistance = 5f;
    [SerializeField] private float minimumDistance = 1f;
    
    private float orbitRadius = 3f;
    
    private bool isOrbiting = true;
    private float mouseX = 0f;
    private float mouseY = 0f;
    
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.O))
        {
            isOrbiting = !isOrbiting;

            if (!isOrbiting)
            {
                transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                transform.rotation = Quaternion.identity;
            }
            else
            {
                orbitRadius = 3f;
            }
        }

        if (isOrbiting)
        {
            if (Input.GetMouseButton(0))
            {
                transform.LookAt(lookAtTarget);
                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");
                transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
            }

            orbitRadius -= Input.mouseScrollDelta.y / sensitivity;
            orbitRadius = Mathf.Clamp(orbitRadius, minimumDistance, maximumDistance);
            transform.position = lookAtTarget.position - transform.forward * orbitRadius;
        }
    }
}
