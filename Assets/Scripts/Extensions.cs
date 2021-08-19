using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 Round(this Vector3 v, Vector3 origin, float cellSize = 1f)
    {
        v -= origin;
        Vector3 output = new Vector3 (v.x, v.y, v.z) / cellSize;
        output.x = Mathf.Round(output.x);
        output.y = Mathf.Round(output.y);
        output.z = Mathf.Round(output.z);
        output += origin;
        return output;
    }

    public static float Round(this float fl, float incrementSize = 1f)
    {
        float output = fl / incrementSize;
        output = Mathf.Round(output);
        return output * incrementSize;
    }

    public static float NormalizeAngle(this float angle)
    {
        if (angle < 0f)
            angle += 360f;
        else if (angle > 360f)
            angle -= 360f;
        return angle;
    }
}
