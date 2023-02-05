using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

public static class RenameTool
{
    [MenuItem("GameObject/Rename", false, 0)]
    static void Rename() {
        var selection = Selection.gameObjects;
        if(selection.Length != 1) return;

        var name = EditorInputDialog.Show("Rename");
        if(name == null) return;

        var go = selection[0];
        Undo.RecordObject(go, "Rename");
        go.name = name;
    }
}