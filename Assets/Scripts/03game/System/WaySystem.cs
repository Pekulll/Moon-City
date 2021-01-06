using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WayCreator
{
    public static Vector3[] GeneratePath(Vector3 startPoint, Vector3 endPoint, float interPathDistance, float pathObjectWidth)
    {
        if (pathObjectWidth == 0)
        {
            pathObjectWidth = .5f;
            Debug.Log("  <b>[WARN:WayCreator] The width of a path's object can't be set to 0, value change for 0.5.");
        }

        float distanceBetween = Vector3.Distance(startPoint, endPoint);
        int pathObjectNumber = CalculateObjectNumber(distanceBetween, interPathDistance, pathObjectWidth);
        float offset = CalculateOffset(distanceBetween, pathObjectNumber, interPathDistance, pathObjectWidth);

        Vector3[] positions = new Vector3[pathObjectNumber];
        Vector3 unitVector = (endPoint - startPoint) / (pathObjectNumber + offset);

        for(int i = 0; i < pathObjectNumber; i++)
        {
            positions[i] = startPoint + (unitVector * i) + (unitVector * offset);
        }

        return positions;
    }

    private static int CalculateObjectNumber(float distance, float interPathDistance, float pathObjectWidth)
    {
        int objectNumber = (int)(distance / (interPathDistance + pathObjectWidth));
        return objectNumber;
    }

    private static float CalculateOffset(float distance, int objectNumber, float interPathDistance, float pathObjectWidth)
    {
        float effectiveDistance = objectNumber * (interPathDistance + pathObjectWidth);
        float delta = distance - effectiveDistance;
        float offset = Mathf.Abs(delta) / 2;
        return offset;
    }
}
