using System.Collections.Generic;
using UnityEngine;

public class MoveMarkerSphereCheck : MonoBehaviour
{
    public bool CheckEnemyInControlRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.414f);
        bool enemyInControlRange  = false;

        foreach (var col in colliders)
        {
            if (col.CompareTag("EnemyUnit"))
            {
                enemyInControlRange = true;
                continue;
            }
        }

        return enemyInControlRange;
    }
}
