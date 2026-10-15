using UnityEngine;

public class NPOCoverNodes : MonoBehaviour
{
    public bool occupied = false;
    public Transform spotToStand;
    public bool deepCover;

    void Awake()
    {
        spotToStand = transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = occupied ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
