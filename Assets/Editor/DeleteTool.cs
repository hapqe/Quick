using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

[InitializeOnLoad]
public static class DeleteTool
{
    static bool prompting = false;
    static DeleteTool()
    {
        // SceneView.duringSceneGui += OnScene;
    }

    private static void OnScene(SceneView sceneView){
        if(Selection.objects.Length == 0) return;
        
        var e = Event.current;
        var k = e.keyCode;
        var d = e.type == EventType.KeyDown;
        var m = e.type == EventType.MouseDown;
        
        // x
        if(d && k == KeyCode.X) {
            Event.current.Use();
            
            prompting = true;

            return;
        }

        if(!prompting) return;

        // enter, 1, d
        if((d && (k == KeyCode.Return || k == KeyCode.Alpha1 || k == KeyCode.D)) || m && e.button == 0) {
            Event.current.Use();
            prompting = false;
            var selection = Selection.gameObjects;
            
            foreach(var go in selection) {
                Undo.DestroyObjectImmediate(go);
            }
            Collapse();
            return;
        }

        // esc or lmb
        if(d && k == KeyCode.Escape || m && e.button == 1) {
            Event.current.Use();
            prompting = false;
        }
    }
}