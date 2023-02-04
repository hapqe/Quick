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
        
        // alt-g
        if(e.alt && e.type == EventType.KeyDown && e.keyCode == KeyCode.G) {
            e.Use();
            
            var selection = Selection.gameObjects;

            Record();
            foreach(var go in selection) {
                // set position to 0,0,0
                go.transform.localPosition = Vector3.zero;
            }
            Collapse();
        }

        // alt-s
        if(e.alt && e.type == EventType.KeyDown && e.keyCode == KeyCode.S) {
            e.Use();
            
            var selection = Selection.gameObjects;

            Record();
            foreach(var go in selection) {
                // set scale to 1,1,1
                go.transform.localScale = Vector3.one;
            }
            Collapse();
        }

        // alt-r
        if(e.alt && e.type == EventType.KeyDown && e.keyCode == KeyCode.R) {
            e.Use();

            var selection = Selection.gameObjects;

            Record();
            foreach(var go in selection) {
                // set rotation to 0,0,0
                go.transform.localEulerAngles = Vector3.zero;
            }
            Collapse();
        }

        // shift-number
        if(e.shift && e.type == EventType.KeyDown) {
            var number = e.keyCode - KeyCode.Alpha0;
            if(!(number >= 0 && number <= 9))   return;
            
            EditorApplication.ExecuteMenuItem("Edit/Selection/Load Selection " + number);
            if(Selection.gameObjects.Length == 0) return;
            var p = PrefabUtility.InstantiatePrefab(Selection.gameObjects[0]);
            if(p == null) return;

            if(p as GameObject) {
                (p as GameObject).transform.position = Cursor.position;
            }
            Undo.RegisterCreatedObjectUndo(p, "Create Prefab");
        }

        // ctrl-l
        if(e.control && e.type == EventType.KeyDown && e.keyCode == KeyCode.L) {
            e.Use();

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
    }
}