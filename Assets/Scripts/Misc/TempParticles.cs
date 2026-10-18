using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempParticles : MonoBehaviour
{
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private GameObject smokeTrailPrefab;
    private float coneAngle = 15f;
    private int emitNumber = 0;
    private float scaleFactor = 5.3f;

    public void SpawnSmoke()
    {
        StartCoroutine(spawnSmokeTrails(3,2f));
    }

    //Spawns a set number of smoke trails with a slightly random ration
    public IEnumerator spawnSmokeTrails(int numSpawned,float targetLength)
    {
        //list of all smoke trails spawned to be able to remove them at the end
        List<GameObject> trailList = new List<GameObject>();

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

            //instantiate trals
            GameObject trail = Instantiate(smokeTrailPrefab, firePoint);

            //ensure position is correct
            trail.transform.localPosition = new Vector3(0, 0, 0);

            //apply rotation
            trail.transform.localRotation = localConeOffset;

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
