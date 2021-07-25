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

    public static Vector3[] RotatePosition(Vector3[] positions, float angle)
    {
        Debug.Log("<color=#00d9ff>" + angle + " radians</color>");
        
        if (angle < 0)
        {
            Debug.LogError("Angle can't be negative!");
            return positions;
        }

        if (angle > 0)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                float radius = Mathf.Sqrt(Mathf.Pow(positions[i].x, 2) + Mathf.Pow(positions[i].z, 2));
                
                positions[i] = new Vector3(
                    Mathf.Cos(angle) * positions[i].x - Mathf.Sin(angle) * positions[i].z,
                    positions[i].y,
                    -Mathf.Sin(angle) * positions[i].x + Mathf.Cos(angle) * positions[i].z
                );
            }
        }
        
        return positions;
    }

    public static float GetAngle(Vector3 from, Vector3 to, Vector3 axis, EffectManager manager)
    {
        Vector3 vector = to - from;

        float length = Mathf.Sqrt(Mathf.Pow(vector.x, 2) + Mathf.Pow(vector.z, 2));
        float axisLength = Mathf.Sqrt(Mathf.Pow(axis.x, 2) + Mathf.Pow(axis.z, 2));
        
        float cos = (vector.x * axis.x + vector.z * axis.z) / (length * axisLength);
        
        manager.GroundTargetEffect(to, Color.red);
        manager.GroundTargetEffect(from, Color.magenta);
        manager.GroundTargetEffect(from + axis, Color.yellow);
        
        Debug.Log("<color=#00d9ff>" + axis + " // " + length + " meters // " + cos + "</color>");
        return Mathf.Acos(cos);
    }
}
