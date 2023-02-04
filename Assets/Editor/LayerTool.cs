using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

[InitializeOnLoad]
public static class LayerTool
{
    static System.Action perform = () => {
        var selection = Selection.gameObjects;
        if(selection.Length == 0) return;

        int layer = -1;

        if(int.TryParse(name, out var l)){
            layer = l;
        }
        else layer = LayerMask.NameToLayer(name);
        
        if(layer == -1) return;

        foreach(var go in selection) {
            Undo.RecordObject(go, "Layer");
            go.layer = layer;
        }
    };

    static System.Func<Event, bool> trigger = e => {
        return e.type == EventType.KeyDown && e.keyCode == KeyCode.L;
    };
    
    static string name = "";
    static bool typing = false;

    static LayerTool()
    {
        SceneView.duringSceneGui += _ => {
            TypingTool.Typing(perform, trigger, ref name, ref typing);
        };
    }
}