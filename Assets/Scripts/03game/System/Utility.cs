using UnityEngine;

public class Utility
{
    public static Vector3[] GetSquareFormationPositions(int objectCount, int space = 0)
    {
        int yMax = (int)Mathf.Sqrt(objectCount);
        int xMax = objectCount / yMax;

        Vector3[] positions = new Vector3[objectCount];

        for(int y = 0; y < yMax + 1; y++)
        {
            for(int x = 0; x < xMax; x++)
            {
                if (x + y * xMax >= objectCount) return positions;
                positions[x + y * xMax] = new Vector3(x + x * space, 0, y + y * space);
            }
        }

        return positions;
    }
}
