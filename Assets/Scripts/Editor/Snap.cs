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
    [SerializeField] float cellSize;
    [SerializeField] int polarDivisions;
    
    SerializedObject sO;
    SerializedProperty propGridType;
    SerializedProperty propCellSize;
    SerializedProperty propGridOrigin;
    SerializedProperty propPolarDivisions;

    private void OnEnable()
    {
        sO = new SerializedObject(this);
        propGridType = sO.FindProperty("gridType");
        propCellSize = sO.FindProperty("cellSize");
        propGridOrigin = sO.FindProperty("gridOrigin");
        propPolarDivisions = sO.FindProperty("polarDivisions");

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;

        // Load saved configuration
        gridType = (GridType) EditorPrefs.GetInt("SNAP_TOOL_gridType", 0);
        var gridOriginX = EditorPrefs.GetFloat("SNAP_TOOL_gridOriginX", 0);
        var gridOriginY = EditorPrefs.GetFloat("SNAP_TOOL_gridOriginY", 0);
        var gridOriginZ = EditorPrefs.GetFloat("SNAP_TOOL_gridOriginZ", 0);
        gridOrigin = new Vector3(gridOriginX, gridOriginY, gridOriginZ);
        cellSize = EditorPrefs.GetFloat("SNAP_TOOL_cellSize", 1);
        polarDivisions = EditorPrefs.GetInt("SNAP_TOOL_polarDivisions", 24);
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;

        // Save configuration
        EditorPrefs.SetInt("SNAP_TOOL_gridType", (int) gridType);
        EditorPrefs.SetFloat("SNAP_TOOL_gridOriginX", gridOrigin.x);
        EditorPrefs.SetFloat("SNAP_TOOL_gridOriginY", gridOrigin.y);
        EditorPrefs.SetFloat("SNAP_TOOL_gridOriginZ", gridOrigin.z);
        EditorPrefs.SetFloat("SNAP_TOOL_cellSize", cellSize);
        EditorPrefs.SetInt("SNAP_TOOL_polarDivisions", polarDivisions);
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
                Handles.DrawAAPolyLine(gridOrigin, Quaternion.AngleAxis(i * (360f / (float)polarDivisions), Vector3.up) * Vector3.forward * 9 * cellSize);
            }
    }

    private void DrawCartesianGrid(Vector3 gridOrigin, float cellSize)
    {
        for (float x = -10 * cellSize; x <= 10 * cellSize; x += cellSize)
        {
            Handles.DrawAAPolyLine(0.4f, new Vector3(x, 0, -10 * cellSize) + gridOrigin, new Vector3(x, 0, 10 * cellSize) + gridOrigin);
        }
        for (float z = -10 * cellSize; z <= 10 * cellSize; z += cellSize)
        {
            Handles.DrawAAPolyLine(0.4f, new Vector3(- 10 * cellSize, 0, z) + gridOrigin, new Vector3(10 * cellSize, 0, z) + gridOrigin);
        }
    }

    private void OnGUI()
    {
        sO.Update();
            EditorGUILayout.PropertyField(propGridType);
            EditorGUILayout.PropertyField(propGridOrigin);
            EditorGUILayout.PropertyField(propCellSize);
            if (gridType == GridType.Polar)
            {
                EditorGUILayout.PropertyField(propPolarDivisions);
                propPolarDivisions.intValue = Math.Max(2, propPolarDivisions.intValue);
            }
        sO.ApplyModifiedProperties();

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
            float closestRadius = diff.magnitude.Round(cellSize);
            float diffRotation = Mathf.Atan2(diff.z, diff.x);
            float closestRot = diffRotation.Round( (2f * Mathf.PI) / polarDivisions);

            return gridOrigin + (new Vector3(Mathf.Cos(closestRot), 0f, Mathf.Sin(closestRot)) * closestRadius);
        }
        return position;
    }
}
