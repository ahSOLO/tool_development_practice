using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorExtensions
{
    public static Vector3 Round(this Vector3 v, Vector3 origin, float cellSize = 1f)
    {
        v -= origin;

        if (v.x % cellSize >= cellSize / 2)
        {
            v.x += cellSize;
        }
        v.x -= v.x % cellSize;
        if (v.y % cellSize >= cellSize / 2)
        {
            v.y += cellSize;
        }
        v.y -= v.y % cellSize;
        if (v.z % cellSize >= cellSize / 2)
        {
            v.z += cellSize;
        }
        v.z -= v.z % cellSize;

        v += origin;
        return v;
    }
}
