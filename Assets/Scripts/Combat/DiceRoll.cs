using UnityEngine;

    public class DiceRoll : MonoBehaviour
    {
        private Rigidbody rb;
        private bool hasLanded = false;
        [SerializeField]
        private float minForce, maxForce;
        private float forceX, forceY, forceZ;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            forceX = Random.Range(minForce,maxForce);
            forceY = Random.Range(minForce, maxForce);
            forceZ = Random.Range(minForce, maxForce);
            rb.AddTorque(new Vector3(forceX, forceY, forceZ), ForceMode.Impulse);
            Vector3 throwDirection = new Vector3(Random.Range(-1f, 1f), 3f, 0);
            rb.AddForce(throwDirection, ForceMode.Impulse);
    }

        public bool IsStopped()
        {
            return rb.linearVelocity.sqrMagnitude < 0.001f && rb.angularVelocity.sqrMagnitude < 0.001f;
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
            Debug.Log("Rolled a " + topFace);
            return topFace;

            
        }




    }
