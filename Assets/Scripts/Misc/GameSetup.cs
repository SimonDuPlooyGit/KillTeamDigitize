using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GameSetup : MonoBehaviour
{
    public bool setupDone;
    
    // Gameobject references for what will move during setup
    [SerializeField]
    private GameObject board;
    [SerializeField]
    private GameObject navMesh;
    [SerializeField]
    private GameObject[] terrainObjects;
    [SerializeField]
    private GameObject[] unitObjects;
    [SerializeField]
    private OrbitCamera orbitCamera;

    [SerializeField]
    private float lerpDuration = 1.0f;

    private struct PositionData
    {
        public Transform transform;
        public float startY;
        public float targetY;
    }

    private PositionData[] terrainData;
    private PositionData[] unitData;
    private float staggerDelay = 0.15f;

    void Start()
    {
        //no op
    }

    public void InitialiseSetup()
    {
        orbitCamera.ToggleCam();
        terrainData = new PositionData[terrainObjects.Length];
        unitData = new PositionData[unitObjects.Length];

        //Save original ground positions and set new positions for terrain objects
        for (int i = 0; i < terrainObjects.Length; i++)
        {
            float originalY = terrainObjects[i].transform.position.y;

            terrainData[i] = new PositionData
            {
                transform = terrainObjects[i].transform,
                targetY = originalY,
                startY = originalY + 15f
            };

            //Set positions
            terrainObjects[i].transform.position = new Vector3(
                terrainObjects[i].transform.position.x,
                terrainData[i].startY,
                terrainObjects[i].transform.position.z
            );
        }

        //Do the same but for unit objects
        for (int i = 0; i < unitObjects.Length;i++)
        {
            //Disable NavMeshAgent
            NavMeshAgent agent = unitObjects[i].gameObject.GetComponent<NavMeshAgent>();
            if(agent !=null)
            {
                agent.enabled = false;
            }

            float originalY = unitObjects[i].transform.position.y;

            unitData[i] = new PositionData
            {
                transform = unitObjects[i].transform,
                targetY = originalY,
                startY = originalY + 15f
            };

            //Set positions 
            unitObjects[i].transform.position = new Vector3(
                unitObjects[i].transform.position.x,
                unitData[i].startY,
                unitObjects[i].transform.position.z
            );
        }

        StartCoroutine(SetUpGame());
    }
    
    private IEnumerator SetUpGame()
    {
        yield return new WaitForSeconds(1.5f);

        //Drop terrain
        if (terrainData.Length > 0)
        {
            float[] terrainElapsedTimes = new float[terrainData.Length];
            float totalTerrainTime = lerpDuration + (terrainData.Length - 1) * staggerDelay;
            float globalTerrainTime = 0f;

            while (globalTerrainTime < totalTerrainTime)
            {
                globalTerrainTime += Time.deltaTime;

                for (int i = 0; i < terrainData.Length; i++)
                {
                    float startTime = i * staggerDelay;

                    if (globalTerrainTime >= startTime)
                    {
                        terrainElapsedTimes[i] += Time.deltaTime;
                        float t = Mathf.Clamp01(terrainElapsedTimes[i] / lerpDuration);

                        float currentY = Mathf.Lerp(terrainData[i].startY, terrainData[i].targetY, t);
                        Vector3 pos = terrainData[i].transform.position;
                        terrainData[i].transform.position = new Vector3(pos.x, currentY, pos.z);
                    }
                }

                yield return null;
            }

            //Snap terrain peices to final positions 
            for (int i = 0; i < terrainData.Length; i++)
            {
                Vector3 pos = terrainData[i].transform.position;
                terrainData[i].transform.position = new Vector3(pos.x, terrainData[i].targetY, pos.z);
            }
        }

        //Delay between dropping terrain and dropping units
        yield return new WaitForSeconds(0.4f);

        //Drop units
        if (unitData.Length > 0)
        {
            float[] unitElapsedTimes = new float[unitData.Length];
            float totalUnitTime = lerpDuration + (unitData.Length - 1) * staggerDelay;
            float globalUnitTime = 0f;

            while (globalUnitTime < totalUnitTime)
            {
                globalUnitTime += Time.deltaTime;

                for (int i = 0; i < unitData.Length; i++)
                {
                    float startTime = i * staggerDelay;

                    if (globalUnitTime >= startTime)
                    {
                        unitElapsedTimes[i] += Time.deltaTime;
                        float t = Mathf.Clamp01(unitElapsedTimes[i] / lerpDuration);

                        float currentY = Mathf.Lerp(unitData[i].startY, unitData[i].targetY, t);
                        Vector3 pos = unitData[i].transform.position;
                        unitData[i].transform.position = new Vector3(pos.x, currentY, pos.z);
                    }
                }

                yield return null;
            }

            //Snap
            for (int i = 0; i < unitData.Length; i++)
            {
                Vector3 targetPosition = new Vector3(
                    unitData[i].transform.position.x,
                    unitData[i].targetY,
                    unitData[i].transform.position.z
                );

                unitData[i].transform.position = targetPosition;

                //Re enable NavMeshAgent
                NavMeshAgent agent = unitData[i].transform.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = true;
                    agent.Warp(targetPosition);
                }
            }
        }
        orbitCamera.ToggleCam();
    }
}