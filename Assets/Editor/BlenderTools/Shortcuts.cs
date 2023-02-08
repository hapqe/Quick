using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

public static class Shortcuts
{
    [MenuItem("BlenderTools/Transform/Reset Position &g")]
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

    [MenuItem("BlenderTools/Transform/Reset Rotation &r")]
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

    [MenuItem("BlenderTools/Transform/Reset Scale &s")]
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

    [MenuItem("BlenderTools/Transform/Cycle Pivot Mode")]
    static void CyclePivot()
    {
        Pivot.CycleMode();
    }
}