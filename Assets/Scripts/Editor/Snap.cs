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
    
    SerializedObject so;
    SerializedProperty propGridType;
    SerializedProperty propCellSize;
    SerializedProperty propGridOrigin;

    private void OnEnable()
    {
        so = new SerializedObject(this);
        propGridType = so.FindProperty("gridType");
        propCellSize = so.FindProperty("cellSize");
        propGridOrigin = so.FindProperty("gridOrigin");

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
        
        for (float x = -5 * cellSize; x < 10 * cellSize; x += cellSize)
        {
            Handles.DrawAAPolyLine(0.2f, new Vector3(x, 0, -100) + gridOrigin, new Vector3(x, 0, 100) + gridOrigin);
        }
        for (float z = -5 * cellSize; z < 10 * cellSize; z += cellSize)
        {
            Handles.DrawAAPolyLine(0.2f, new Vector3(-100, 0, z) + gridOrigin, new Vector3(100, 0, z) + gridOrigin);
        }
    }

    private void OnGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(propGridType);
        EditorGUILayout.PropertyField(propGridOrigin);
        EditorGUILayout.PropertyField(propCellSize);
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
            gO.transform.position = gO.transform.position.Round(gridOrigin, cellSize);
        }
    }
}
