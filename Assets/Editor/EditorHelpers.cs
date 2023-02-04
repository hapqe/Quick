using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorHelpers
{
    public static bool isInAction {
        get {
            return MoveTool.moving;
        }
    }
    
    public static void Record(System.Func<GameObject, Object> action = null) {
        action = action ?? (go => go.transform);
        var selection = Selection.gameObjects;
        foreach(var go in selection) {
            Undo.RecordObject(action(go), "Transform");
        }
    }

    public static void Collapse() {
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
    }

    public static bool IsLetter(KeyCode key) {
        return key >= KeyCode.A && key <= KeyCode.Z;
    }

    public static bool IsNumber(KeyCode key) {
        return key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9;
    }

    public static bool IsLetterOrNumber(KeyCode key) {
        return IsLetter(key) || IsNumber(key);
    }
    
    public static T GetDrawer<T>() where T : MonoBehaviour, IGizmosDrawer {
        var drawer = Object.FindObjectOfType<T>();
        if(drawer == null) {
            drawer = new GameObject(nameof(T)).AddComponent<T>();
            drawer.transform.hideFlags = HideFlags.HideInHierarchy;
        }
        return drawer;
    }
}
