using UnityEngine;

//-----DiceRoll Class-----
public class DiceRoll : MonoBehaviour
{
    private Rigidbody _rb;
    private bool hasLanded = false;
    [SerializeField]
    private float minForce, maxForce;
    private float _forceX, _forceY, _forceZ;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _forceX = Random.Range(minForce,maxForce);
        _forceY = Random.Range(minForce, maxForce);
        _forceZ = Random.Range(minForce, maxForce);
        _rb.AddTorque(new Vector3(_forceX, _forceY, _forceZ), ForceMode.Impulse);
        Vector3 throwDirection = new Vector3(Random.Range(-1f, 1f), 3f, 0);
        _rb.AddForce(throwDirection, ForceMode.Impulse);
    }
        
    public bool IsStopped()
    {
        return _rb.linearVelocity.sqrMagnitude < 0.001f && _rb.angularVelocity.sqrMagnitude < 0.001f;
    }

    public int GetUpwardFace()
    {
        Vector3[] directions = new Vector3[]
        {
            transform.forward, //Face 1
            -transform.forward, //Face 6
            transform.up, //Face 4
            -transform.up, //Face 3
            transform.right, //Face 2
            -transform.right //Face 5
        };

        int[] faceValues = new int[] { 1, 6, 4, 3, 2, 5 };

        float maxDot = -Mathf.Infinity;
        int topFace = 1;

        for (int i = 0; i < directions.Length; i++)
        {
            // Compare local direction to global UP (0, 1, 0)
            float dot = Vector3.Dot(directions[i], Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                topFace = faceValues[i];
            }
        }
        //Debug.Log("Rolled a " + topFace);
        return topFace;
    }

    public void SetDesiredFace(int targetFace)
    {
        // 1. Identify which local direction corresponds to targetFace
        Vector3 localDir = Vector3.forward; // Default to face 1

        switch (targetFace)
        {
            case 1: localDir = Vector3.forward; break;
            case 6: localDir = -Vector3.forward; break;
            case 4: localDir = Vector3.up; break;
            case 3: localDir = -Vector3.up; break;
            case 2: localDir = Vector3.right; break;
            case 5: localDir = -Vector3.right; break;
            default:
                localDir = -Vector3.forward;
                break;
        }

        // 2. Calculate the rotation required to align localDir to world UP (0, 1, 0)
        Quaternion alignRotation = Quaternion.FromToRotation(transform.TransformDirection(localDir), Vector3.up);

        // 3. Apply the rotation and freeze physics
        transform.rotation = alignRotation * transform.rotation;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
}