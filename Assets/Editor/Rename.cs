using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

[InitializeOnLoad]
public static class RenameTool
{
    static System.Action perform = () => {
        var selection = Selection.gameObjects;
        if(selection.Length != 1) return;

        var go = selection[0];
        Undo.RecordObject(go, "Rename");
        go.name = name;
    };

    static System.Func<Event, bool> trigger = e => {
        return e.type == EventType.KeyDown && e.keyCode == KeyCode.F2;
    };
    
    static string name = "";
    static bool typing = false;

    static RenameTool()
    {
        SceneView.duringSceneGui += _ => {
            TypingTool.Typing(perform, trigger, ref name, ref typing);
        };
    }
}