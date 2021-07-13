using System;
using UnityEngine;

public class Utility
{
    public static Vector3[] GetFormationPositions(int formation, int objectCount, int space = 0)
    {
        if (formation == 0)
        {
            return Utility.GetSquareFormationPositions(objectCount, space);
        } else if (formation == 1)
        {
            return Utility.GetLineFormationPositions(objectCount, space);
        } else if (formation == 2)
        {
            return Utility.GetTriangleFormationPositions(objectCount, space);
        }

        throw new Exception("Formation ID cannot be proceed!");
    }
    
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

    private static Vector3[] GetLineFormationPositions(int objectCount, int space)
    {
        int yMax = 2;
        int xMax = objectCount / yMax + 1;

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
    
    private static Vector3[] GetTriangleFormationPositions(int objectCount, int space)
    {
        //throw new System.NotImplementedException();

        int lineLength = 1;
        int unitPlaced = 0;
        
        Vector3[] positions = new Vector3[objectCount];

        for (int y = 0; y < objectCount; y++)
        {
            for (int x = 0; x < lineLength; x++)
            {
                if (unitPlaced >= objectCount) return positions;
                
                float realLineLength = lineLength + lineLength * space;
                positions[unitPlaced] = new Vector3(x + x * space - (realLineLength - 0) / 2, 0, y + y * space);
                unitPlaced++;
            }

            lineLength++;
        }

        return positions;
    }
}
