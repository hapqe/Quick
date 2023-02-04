using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MoveTool
{
    static bool flipYZ = true;
    
    static MoveTool()
    {
        SceneView.duringSceneGui += OnScene;
    }

    static bool moving = false;
    static Vector3[] initial;
    static Vector3 start;
    static Vector3 mean;

    static Vector3 mask;
    static string input = "";

    private static void OnScene(SceneView sceneView)
    {
        Event e = Event.current;

        if(e.alt) return;

        var sel = Selection.gameObjects;

        // g
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.G)
        {
            if (sel.Length == 0) return;
            moving = true;

            // undo
            foreach (var go in sel)
            {
                Undo.RecordObject(go.transform, "Move");
            }

            initial = new Vector3[sel.Length];
            for (int i = 0; i < sel.Length; i++)
            {
                initial[i] = sel[i].transform.position;
            }

            mean = MeanSelection();
            start = GetMousePosition(mean);
            EditorHelpers.GetDrawer<MoveGizmos>().mean = mean;

            mask = Vector3.one;

            input = "";
        }

        if (!moving) return;

        var delta = GetMousePosition(mean) - start;
        EditorHelpers.GetDrawer<MoveGizmos>().delta = delta;

        for (int i = 0; i < sel.Length; i++)
        {
            if(input == "")
            sel[i].transform.position = initial[i] + Vector3.Scale(delta, mask);
            else
            {
                float d = float.Parse(input);
                if(mask.magnitude != 1f)
                // x axis
                sel[i].transform.position = initial[i] + new Vector3(d, 0, 0);
                else
                sel[i].transform.position = initial[i] + mask * d;
            }
        }

        // rmb or esc
        if (e.type == EventType.MouseDown && e.button == 1 || e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            e.Use();
            moving = false;

            for (int i = 0; i < sel.Length; i++)
            {
                sel[i].transform.position = initial[i];
            }

            EditorHelpers.GetDrawer<MoveGizmos>().showAll = false;
            EditorHelpers.GetDrawer<MoveGizmos>().mask = Vector3.one;

            return;
        }

        // lmb or enter
        if (e.type == EventType.MouseDown && e.button == 0  || e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
        {
            e.Use();

            // undo
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            moving = false;

            EditorHelpers.GetDrawer<MoveGizmos>().showAll = false;
            EditorHelpers.GetDrawer<MoveGizmos>().mask = Vector3.one;

            return;
        }

        // x
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.X)
        {
            mask = Vector3.right;
        }
        // y
        if (e.type == EventType.KeyDown && e.keyCode == GetKey(KeyCode.Y))
        {
            mask = Vector3.up;
        }
        // z
        if (e.type == EventType.KeyDown && e.keyCode == GetKey(KeyCode.Z))
        {
            mask = Vector3.forward;
        }

        // shift-x
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.X && e.shift)
        {
            mask = new Vector3(0, 1, 1);
        }
        // shift-y
        if (e.type == EventType.KeyDown && e.keyCode == GetKey(KeyCode.Y) && e.shift)
        {
            mask = new Vector3(1, 0, 1);
        }
        // shift-z
        if (e.type == EventType.KeyDown && e.keyCode == GetKey(KeyCode.Z) && e.shift)
        {
            mask = new Vector3(1, 1, 0);
        }

        // mmb
        if (e.type == EventType.MouseDown && e.button == 2)
        {
            e.Use();
        }
        // mmb up
        if (e.type == EventType.MouseUp && e.button == 2)
        {
            EditorHelpers.GetDrawer<MoveGizmos>().showAll = false;
        }
        // mmb drag
        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            EditorHelpers.GetDrawer<MoveGizmos>().showAll = true;
            
            var d = delta.normalized;
            var axes = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };
            var best = 0;
            var bestDot = 0f;
            for (int i = 0; i < 3; i++)
            {
                var normal = Camera.current.transform.forward;
                var p = Vector3.ProjectOnPlane(axes[i], normal);
                p.Normalize();
                
                var dot = Mathf.Abs(Vector3.Dot(d, p));
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = i;
                }
            }
            var axis = axes[best];
            if(e.shift) 
                mask = Vector3.one - axis;
            else
                mask = axis;

            e.Use();
        }

        EditorHelpers.GetDrawer<MoveGizmos>().mask = mask;

        // input
        if (e.type == EventType.KeyDown)
        {
            e.Use();
            
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
        }
    }

    static Vector3 MeanSelection()
    {
        var sel = Selection.gameObjects;
        var mean = Vector3.zero;
        foreach (var go in sel)
        {
            mean += go.transform.position;
        }
        mean /= sel.Length;
        return mean;
    }

    static Vector3 GetMousePosition(Vector3 mean)
    {
        var e = Event.current;
        var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        var normal = Camera.current.transform.forward;
        var plane = new Plane(normal, mean);

        float dist;
        plane.Raycast(ray, out dist);
        var pos = ray.GetPoint(dist);
        return pos;
    }

    static KeyCode GetKey(KeyCode key)
    {
        Debug.Assert(key == KeyCode.Y || key == KeyCode.Z);
        return flipYZ ? (key == KeyCode.Y ? KeyCode.Z : KeyCode.Y) : key;
    }
}