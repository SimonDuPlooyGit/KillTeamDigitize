using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeTrails : MonoBehaviour
{
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private GameObject smokeTrailPrefab;
    private float coneAngle = 6f;
    private int emitNumber = 0;
    private float scaleFactor = 5.3f;

    public void SpawnSmoke(int numSpawned, Transform startPoint, Transform endPoint)
    {
        StartCoroutine(spawnSmokeTrails(numSpawned,startPoint,endPoint));
    }

    //Spawns a set number of smoke trails with a slightly random ration
    public IEnumerator spawnSmokeTrails(int numSpawned,Transform startPoint, Transform endPoint)
    {
        
        //Make target length equal to distance between units
        float targetLength = Vector3.Distance(startPoint.position, endPoint.position);
        Debug.Log("Target length: " + targetLength);
        //list of all smoke trails spawned to be able to remove them at the end
        List<GameObject> trailList = new List<GameObject>();

        Vector3 localDirection = firePoint.InverseTransformDirection(endPoint.position - startPoint.position);
        Quaternion baseLocalRotation = Quaternion.LookRotation(localDirection);

        //calculate correct Z scale for particle object to achieve desired distance
        targetLength -= 0.08f;
        float scaledLength = targetLength / scaleFactor;

        //emit extra particles based on extra length
        emitNumber = (int)targetLength * 50;

        for (int i = 0; i < numSpawned; i++)
        {
            //random offset for rotation
            Vector2 randomPoint = Random.insideUnitCircle * coneAngle;
            Quaternion localConeOffset = Quaternion.Euler(randomPoint.x, randomPoint.y, 0f);

            Quaternion finalLocalRotation = baseLocalRotation * localConeOffset;

            //instantiate trals
            GameObject trail = Instantiate(smokeTrailPrefab, firePoint);

            //ensure position is correct
            trail.transform.localPosition = new Vector3(0, 0, 0);

            //apply rotation
            trail.transform.localRotation = finalLocalRotation;

            //apply scaling
            trail.transform.localScale = new Vector3(
                trail.transform.localScale.x, 
                trail.transform.localScale.y, 
                trail.transform.localScale.z + scaledLength);

            trailList.Add(trail);

            //play particle effect
            ParticleSystem particles = trail.GetComponentInChildren<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
                particles.Emit(emitNumber);
            }

        }

        //waiting and then deleting all trails
        yield return new WaitForSeconds(1);
        foreach (GameObject trail in trailList)
        {
            Destroy(trail);
        }

    }
}
