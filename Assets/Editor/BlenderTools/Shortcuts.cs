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
}