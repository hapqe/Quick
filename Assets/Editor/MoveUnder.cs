using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

[InitializeOnLoad]
public static class MoveUnder
{
    static bool typing = false;
    static string name = "";
    static MoveUnder()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private static void OnScene(SceneView sceneView){
        var e = Event.current;
        var k = e.keyCode;
        var d = e.type == EventType.KeyDown;
        
        // m
        if(d && k == KeyCode.M) {
            Event.current.Use();
            
            var selection = Selection.gameObjects;
            
            if(selection.Length == 0) return;

            typing = true;

            return;
        }

        if(!typing) return;

        if(d && EditorHelpers.IsLetter(k)) {
            Event.current.Use();
            var c = Event.current.keyCode.ToString();
            if(!e.shift) c = c.ToLower();
            name += c;
        }
        if(d && EditorHelpers.IsNumber(k)) {
            Event.current.Use();
            name += Event.current.keyCode.ToString().Substring(5);
        }
        if(d && k == KeyCode.Backspace) {
            Event.current.Use();
            if(name.Length > 0) name = name.Substring(0, name.Length - 1);
        }

        // esc or rmb
        if(d && (k == KeyCode.Escape || e.isMouse && e.button == 1)) {
            Event.current.Use();
            typing = false;
            name = "";
        }

        // enter or lmb
        if(d && (k == KeyCode.Return || e.isMouse && e.button == 0)) {
            Event.current.Use();
            typing = false;
            var selection = Selection.gameObjects;
            var parent = GameObject.Find(name);
            if(parent == null) {
                parent = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(parent, "Move Under");
            }
            foreach(var go in selection) {
                Undo.SetTransformParent(go.transform, parent.transform, "Move Under");
            }
            Selection.activeGameObject = null;
            Selection.activeGameObject = parent;
            Collapse();
            name = "";
        }

        if(!(e.type == EventType.Layout || e.type == EventType.Repaint))
        e.Use();
    }
}