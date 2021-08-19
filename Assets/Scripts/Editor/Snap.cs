using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Snap : EditorWindow
{
    [MenuItem("Tools/Snap Tool")]
    public static void OpenSnapWindow() => GetWindow<Snap>("Snap Tool");
    public enum GridType { Cartesian, Polar};
    [SerializeField] GridType gridType = GridType.Cartesian;
    [SerializeField] Vector3 gridOrigin;
    [SerializeField] float cellSize = 1;
    [SerializeField] int polarDivisions;
    
    SerializedObject so;
    SerializedProperty propGridType;
    SerializedProperty propCellSize;
    SerializedProperty propGridOrigin;
    SerializedProperty propPolarDivisions;

    private void OnEnable()
    {
        so = new SerializedObject(this);
        propGridType = so.FindProperty("gridType");
        propCellSize = so.FindProperty("cellSize");
        propGridOrigin = so.FindProperty("gridOrigin");
        propPolarDivisions = so.FindProperty("polarDivisions");

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView obj)
    {
        if (Event.current.type != EventType.Repaint)
            return;
        
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        
        if (gridType == GridType.Cartesian)
        {
            DrawCartesianGrid(gridOrigin, cellSize);
        }

        else if (gridType == GridType.Polar)
        {
            DrawPolarGrid(gridOrigin, cellSize);
        }
    }

    private void DrawPolarGrid(Vector3 gridOrigin, float cellSize)
    {
        for (float i = 1 * cellSize; i < 10 * cellSize; i += cellSize)
        {
            Handles.DrawWireDisc(gridOrigin, Vector3.up, i);
        }

        if (polarDivisions >= 2)
            for (float i = 1 * cellSize; i <= polarDivisions; i++)
            {
                Handles.DrawAAPolyLine(gridOrigin, Quaternion.AngleAxis(i * (360f / (float)polarDivisions), Vector3.up) * Vector3.forward * 10);
            }
    }

    private void DrawCartesianGrid(Vector3 gridOrigin, float cellSize)
    {
        for (float x = -5 * cellSize; x < 10 * cellSize; x += cellSize)
        {
            Handles.DrawAAPolyLine(0.2f, new Vector3(x, 0, -10) + gridOrigin, new Vector3(x, 0, 10) + gridOrigin);
        }
        for (float z = -5 * cellSize; z < 10 * cellSize; z += cellSize)
        {
            Handles.DrawAAPolyLine(0.2f, new Vector3(-10, 0, z) + gridOrigin, new Vector3(10, 0, z) + gridOrigin);
        }
    }

    private void OnGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(propGridType);
        EditorGUILayout.PropertyField(propGridOrigin);
        EditorGUILayout.PropertyField(propCellSize);
        if (gridType == GridType.Polar)
        {
            EditorGUILayout.PropertyField(propPolarDivisions);
            propPolarDivisions.intValue = Math.Max(2, propPolarDivisions.intValue);
        }
        so.ApplyModifiedProperties();

        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            var snapButton = GUILayout.Button("Snap Selection");
            if (snapButton)
                SnapSelection();
        }
    }

    public void SnapSelection()
    {
        foreach (GameObject gO in Selection.gameObjects)
        {
            Undo.RecordObject(gO.transform, "Snap Objects");
            gO.transform.position = GetSnappedPosition(gridType, gO.transform.position, gridOrigin, cellSize, polarDivisions);
        }
    }

    Vector3 GetSnappedPosition(GridType gridType, Vector3 position, Vector3 gridOrigin, float cellSize, int polarDivisions = 0)
    {
        if (gridType == GridType.Cartesian)
            return position.Round(gridOrigin, cellSize);
        else if (gridType == GridType.Polar)
        {
            if (polarDivisions < 2)
                return gridOrigin;
            
            Vector3 diff = position - gridOrigin;
            float closestRadius = float.MaxValue;
            float closestSM = float.MaxValue;
            float closestLineRot = 0f;
            float closestDot = -1f;

            for (float i = 1 * cellSize; i < 10 * cellSize; i += cellSize)
            {
                var diffSM = (diff - (diff.normalized * i)).sqrMagnitude;
                if ( diffSM < closestSM )
                {
                    closestRadius = i;
                    closestSM = diffSM;
                }
                else
                    break;
            }
            
            for (float i = 0; i <= polarDivisions; i += cellSize)
            {
                float rot = i * (360f / (float)polarDivisions);
                float dot = Vector3.Dot(diff.normalized, Quaternion.AngleAxis(rot, Vector3.up) * Vector3.forward);
                if (dot > closestDot)
                {
                    closestDot = dot;
                    closestLineRot = rot;
                }    
            }

            return gridOrigin + (Quaternion.AngleAxis(closestLineRot, Vector3.up) * Vector3.forward * closestRadius);
        }
        return position;
    }
}
