using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public static class EditorHelpers
{
    const bool flipYZ = true;
    
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
        SceneVisibilityManager.instance.DisablePicking(drawer.gameObject, true);
        return drawer;
    }

    public static Vector3 GetMask(Event e, Vector3 delta, Vector3 pos, Vector3[] orientation, out MoveMode mode, out Vector3 normal)
    {
        delta.Normalize();
        var global = Tools.pivotRotation == PivotRotation.Global;

        var globalAxes = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };

        var axes = global ?
            globalAxes
            : orientation;

        var best = 0;
        var bestDot = 0f;
        for (int i = 0; i < 3; i++)
        {
            var center = (Vector2)Camera.current.WorldToScreenPoint(pos);
            var a = (Vector2)Camera.current.WorldToScreenPoint(pos + axes[i]);
            var p = (Vector2)Camera.current.WorldToScreenPoint(pos + delta);

            var dot = Mathf.Abs(Vector2.Dot((p - center).normalized, (a - center).normalized));
            if (dot >= bestDot)
            {
                bestDot = dot;
                best = i;
            }
        }

        var axis = globalAxes[best];
        normal = axis;
        if (e.shift) {
            mode = MoveMode.Plane;
            return Vector3.one - axis;
        }
        else {
            mode = MoveMode.Axis;
            return axis;
        }
    }

    public static void AppendEvent(Event e, ref string input)
    {
        if(e.type != EventType.KeyDown)
            return;
        
        // append to input
        if (e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9)
            input += (char)('0' + (e.keyCode - KeyCode.Alpha0));
        if (e.keyCode >= KeyCode.Keypad0 && e.keyCode <= KeyCode.Keypad9)
            input += (char)('0' + (e.keyCode - KeyCode.Keypad0));
        if (e.keyCode == KeyCode.Period)
            input += '.';
        if (e.keyCode == KeyCode.Comma)
            input += ',';
        // backspace
        if (e.keyCode == KeyCode.Backspace)
        {
            if (input.Length > 0)
                input = input.Substring(0, input.Length - 1);
        }

        e.Use();
    }

    public static Vector3 GetPlanePosition(Vector2 point, Vector2 mouse)
    {
        var e = Event.current;
        var ray = HandleUtility.GUIPointToWorldRay(mouse);
        var normal = Camera.current.transform.forward;
        var plane = new Plane(normal, point);

        float dist;
        plane.Raycast(ray, out dist);
        var pos = ray.GetPoint(dist);
        return pos;
    }

    public static KeyCode GetKey(KeyCode key)
    {
        Debug.Assert(key == KeyCode.Y || key == KeyCode.Z);
        return flipYZ ? (key == KeyCode.Y ? KeyCode.Z : KeyCode.Y) : key;
    }
}
