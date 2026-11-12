using UnityEngine;

//-----SaveResolver (Decides how defence dice are spent against retained attack hits when shooting)-----
public static class SaveResolver
{
    //Rules:
    //  A crit save blocks one crit hit or one normal hit
    //  A normal save blocks one normal hit
    //  Two normal saves together block one crit hit
    //
    // Tries every way of splitting the saves and keeps the one that lets through the least damage.
    // So two normal saves go on a crit when that stops more damage than blocking two normal hits would,
    // and on a tie the saves go on crits first.
    public static void Resolve(
        int crits, int normals,
        int critSaves, int normalSaves,
        int dmgNorm, int dmgCrit,
        out int critsLeft, out int normalsLeft)
    {
        crits = Mathf.Max(0, crits);
        normals = Mathf.Max(0, normals);
        critSaves = Mathf.Max(0, critSaves);
        normalSaves = Mathf.Max(0, normalSaves);

        int bestDamage = int.MaxValue;
        critsLeft = crits;
        normalsLeft = normals;

        //How many crit saves are used on crit hits (the rest block normal hits). Highest first so ties favour blocking crits
        for (int critSavesOnCrits = Mathf.Min(critSaves, crits); critSavesOnCrits >= 0; critSavesOnCrits--)
        {
            int critsAfterCritSaves = crits - critSavesOnCrits;
            int critSavesOnNormals = Mathf.Min(critSaves - critSavesOnCrits, normals);
            int normalsAfterCritSaves = normals - critSavesOnNormals;

            //How many crits are blocked by a pair of normal saves. The remaining normal saves block single normal hits
            for (int pairs = Mathf.Min(normalSaves / 2, critsAfterCritSaves); pairs >= 0; pairs--)
            {
                int c = critsAfterCritSaves - pairs;
                int n = Mathf.Max(0, normalsAfterCritSaves - (normalSaves - pairs * 2));
                int damage = c * dmgCrit + n * dmgNorm;

                if (damage < bestDamage)
                {
                    bestDamage = damage;
                    critsLeft = c;
                    normalsLeft = n;
                }
            }
        }
    }
}
