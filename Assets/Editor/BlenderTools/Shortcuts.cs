using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

[InitializeOnLoad]
public static class Shortcuts
{
    static Shortcuts()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private static void OnScene(SceneView sceneView){
        Event e = Event.current;
        var k = e.keyCode;
        var d = e.type == EventType.KeyDown;
        var m = e.type == EventType.MouseDown;
        
        // shift-number
        if(e.shift && d) {
            var number = k - KeyCode.Alpha0;
            if(!(number >= 0 && number <= 9))   return;
            
            Spawn(number);
        }
    }

    [MenuItem("Edit/Reset Position &g")]
    static void ResetPosition()
    {
        var selection = Selection.gameObjects;

        Record();
        foreach (var go in selection)
        {
            // set position to 0,0,0
            go.transform.localPosition = Vector3.zero;
        }
        Collapse();
    }

    [MenuItem("Edit/Reset Rotation &r")]
    static void ResetRotation()
    {
        var selection = Selection.gameObjects;

        Record();
        foreach (var go in selection)
        {
            // set rotation to 0,0,0
            go.transform.localRotation = Quaternion.identity;
        }
        Collapse();
    }

    [MenuItem("Edit/Reset Scale &s")]
    static void ResetScale()
    {
        var selection = Selection.gameObjects;

        Record();
        foreach (var go in selection)
        {
            // set scale to 1,1,1
            go.transform.localScale = Vector3.one;
        }
        Collapse();
    }

    [MenuItem("Edit/Copy Materials %L")]
    static void CopyMaterials()
    {
        var sel = Selection.gameObjects;
        if(sel.Length == 0) return;

        var active = Selection.activeGameObject;

        var activeMat = active.GetComponent<Renderer>()?.sharedMaterial;

        if(activeMat == null) return;

        foreach(var go in sel) {
            var rend = go.GetComponent<Renderer>();
            if(rend == null) continue;

            Undo.RecordObject(rend, "Set Material");

            rend.sharedMaterial = activeMat;
        }

        Collapse();
    }

    static void Spawn(int number) {
        EditorApplication.ExecuteMenuItem("Edit/Selection/Load Selection " + number);
        if(Selection.gameObjects.Length == 0) return;
        var p = PrefabUtility.InstantiatePrefab(Selection.gameObjects[0]);
        if(p == null) return;

        if(p as GameObject) {
            (p as GameObject).transform.position = Cursor.position;
        }
        Undo.RegisterCreatedObjectUndo(p, "Create Prefab");

        Selection.activeGameObject = null;
        if(p as GameObject != null)
        Selection.activeGameObject = p as GameObject;
    }
}