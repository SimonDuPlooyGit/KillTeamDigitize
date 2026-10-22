using System.Collections.Generic;
using UnityEngine;

public class MoveMarkerSphereCheck : MonoBehaviour
{
    public bool CheckEnemyInControlRange(string unitTag)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.414f);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Terrain")) continue;

            BaseOperativeClass targetOp = col.GetComponentInParent<BaseOperativeClass>();
            if (targetOp != null && !targetOp.dead)
            {
                //If the detected unit's tag is different, it is an enemy
                if (!col.CompareTag(unitTag))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
