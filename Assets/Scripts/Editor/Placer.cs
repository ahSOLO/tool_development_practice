using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Placer : EditorWindow
{
    [MenuItem("Tools/Placer")]
    public static void OpenPlacer() => GetWindow<Placer>();

    [SerializeField] float radius = 2f;
    [SerializeField] int spawnCount = 8;
    [SerializeField] GameObject prefab;
    [SerializeField] GameObject prefabContainer;

    SerializedObject sO;
    SerializedProperty propRadius;
    SerializedProperty propSpawnCount;
    SerializedProperty propPrefab;
    SerializedProperty propPrefabContainer;

    private void OnEnable()
    {
        sO = new SerializedObject(this);
        propRadius = sO.FindProperty("radius");
        propSpawnCount = sO.FindProperty("spawnCount");
        propPrefab = sO.FindProperty("prefab");
        propPrefabContainer = sO.FindProperty("prefabContainer");

        SceneView.duringSceneGui += DuringSceneGui;

        // Load saved configurations
        radius = EditorPrefs.GetFloat("PLACER_TOOL_radius", 2f);
        spawnCount = EditorPrefs.GetInt("PLACER_TOOL_spawnCount", 8);
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString("PLACER_TOOL_prefab", "Assets/Prefabs/Spawnable.prefab"));
        prefabContainer = GameObject.Find(EditorPrefs.GetString("PLACER_TOOL_prefabContainer", "SpawnableContainer"));
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGui;
        
        // Save configurations
        EditorPrefs.SetFloat("PLACER_TOOL_radius", radius);
        EditorPrefs.SetInt("PLACER_TOOL_spawnCount", spawnCount);
        EditorPrefs.SetString("PLACER_TOOL_prefab", AssetDatabase.GetAssetPath(prefab));
        EditorPrefs.SetString("PLACER_TOOL_prefabContainer", prefabContainer.name);
    }

    private void OnGUI()
    {
        sO.Update();
        EditorGUILayout.PropertyField(propRadius);
        radius = Mathf.Clamp(radius, 0.1f, 10f);
        EditorGUILayout.PropertyField(propSpawnCount);
        spawnCount = Mathf.Clamp(spawnCount, 1, 99);
        EditorGUILayout.PropertyField(propPrefab);
        EditorGUILayout.PropertyField(propPrefabContainer);
        if (sO.ApplyModifiedProperties())
            SceneView.RepaintAll();
    }

    private void DuringSceneGui(SceneView sV)
    {
        Camera camT = sV.camera;

        if (Event.current.type == EventType.MouseMove)
            sV.Repaint();

        if (Event.current.type == EventType.ScrollWheel && (Event.current.modifiers & EventModifiers.Alt) != 0)
        {
            float scrollDir = Mathf.Sign(Event.current.delta.y);
            sO.Update();
            propRadius.floatValue *= 1 + (scrollDir * 0.1f);
            sO.ApplyModifiedProperties();
            Repaint();
            Event.current.Use();
        }

        // Alternatively you can use HandleUtility.GUIPointToWorldRay
        Ray ray = new Ray(camT.transform.position, camT.ScreenToWorldPoint(new Vector3(Event.current.mousePosition.x, camT.pixelHeight - Event.current.mousePosition.y, 100f)) - camT.transform.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Handles.DrawAAPolyLine(4f, hit.point, hit.point + hit.normal);
            // Handles.DrawWireDisc(hit.point, hit.normal, radius);

            Vector3 tangent = Vector3.Cross(hit.normal, camT.transform.forward);
            Vector3 bitangent = Vector3.Cross(hit.normal, tangent);

            int circleDetail = 64;
            RaycastHit prevCircDrawHit;
            Physics.Raycast(hit.point + tangent * radius + hit.normal * 4f, -hit.normal, out prevCircDrawHit, 50f);
            for (int i = 1; i < circleDetail + 1; i++)
            {
                var pointOnCircle = hit.point + Quaternion.AngleAxis(i * 360 / circleDetail, hit.normal) * (tangent * radius);
                Ray circDrawRay = new Ray(pointOnCircle + hit.normal * 4f, -hit.normal);
                RaycastHit circDrawHit;
                if (!Physics.Raycast(circDrawRay, out circDrawHit, 50f))
                {
                    circDrawHit.point = pointOnCircle;
                }
                Handles.DrawAAPolyLine(2f, prevCircDrawHit.point, circDrawHit.point);
                prevCircDrawHit = circDrawHit;
            }

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
            {
                int tries = 0;

                for (int i = 0; i < spawnCount; i++)
                {
                    var randPoint = UnityEngine.Random.insideUnitCircle * radius;
                    Ray spawnRay = new Ray(hit.point + (hit.normal * 4f) + (tangent * randPoint.x) + (bitangent * randPoint.y), -hit.normal);
                    if (Physics.Raycast(spawnRay, out RaycastHit spawnHit))
                    {
                        var newGO = Instantiate<GameObject>(prefab, spawnHit.point, Quaternion.Euler(0f, UnityEngine.Random.Range(-180, 180), 0f), prefabContainer.transform);
                        Undo.RegisterCreatedObjectUndo(newGO, "Spawn Object");
                    }
                    else
                    {
                        i--;
                        tries++;
                    }
                    if (tries > 100)
                        break;
                }
            }

            HandleUtility.AddDefaultControl(0);
        }
    }
}
