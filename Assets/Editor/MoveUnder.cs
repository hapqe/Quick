using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

[InitializeOnLoad]
public static class MoveUnder
{
    static System.Action perform = () => {
        var selection = Selection.gameObjects;
        var parent = GameObject.Find(name);
        if (parent == null)
        {
            parent = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(parent, "Move Under");
        }
        foreach (var go in selection)
        {
            Undo.SetTransformParent(go.transform, parent.transform, "Move Under");
        }
        Selection.activeGameObject = null;
        Selection.activeGameObject = parent;
    };

    static System.Func<Event, bool> trigger = e => {
        return e.type == EventType.KeyDown && e.keyCode == KeyCode.M;
    };
    
    static string name = "";
    static bool typing = false;

    static MoveUnder()
    {
        SceneView.duringSceneGui += _ => {
            TypingTool.Typing(perform, trigger, ref name, ref typing);
        };
    }
}