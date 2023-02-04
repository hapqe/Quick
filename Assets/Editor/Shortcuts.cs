using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class Shortcuts
{
    static Shortcuts()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private static void Record() {
        var selection = Selection.gameObjects;
        foreach(var go in selection) {
            Undo.RecordObject(go.transform, "Transform");
        }
    }

    private static void Collapse() {
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
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
    }
}