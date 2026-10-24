using UnityEngine;

public class MoveMarkerSphereCheck : MonoBehaviour
{
    public bool CheckEnemyInControlRange(string unitTag)
    {
        Debug.Log($"Called CheckEnemyInControlRange from {name} with tag {gameObject.tag} to check against opposite of {unitTag}");
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, 0.414f);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Terrain") || col.CompareTag("Board")) continue;

            BaseOperativeClass targetOp = col.GetComponentInParent<BaseOperativeClass>();
            if (targetOp != null && !targetOp.dead)
            {
                Debug.Log($"The check found {targetOp.name} with tag {targetOp.tag}");
                
                //If the detected unit's tag is the same, skip it
                if (targetOp.CompareTag(unitTag))
                {
                    Debug.Log($"{targetOp.name}'s tag {targetOp.tag} is the same as {name}'s tag {gameObject.tag}");
                    continue;
                }
                Debug.Log($"Found valid enemy: {targetOp.name} with tag {col.tag}");
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, 0.414f);
    }
}
