using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

public static class MoveUnder
{
    [MenuItem("GameObject/Move Under", false, 0)]
    static void Move() {
        var name = EditorInputDialog.Show("Move Under");
        if(name == null) return;
        
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
    }
}