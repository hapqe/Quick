using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorState;

[InitializeOnLoad]
public class MoveTool : TransformTool
{
    public override Predicate<Event> trigger => e =>
        e.type == EventType.KeyDown
        && e.keyCode == KeyCode.G
        && Selection.transforms.Length > 0
        && !e.alt;

    static MoveTool()
    {
        tools.Add(new MoveTool());
    }

    public override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        var e = Event.current;

        var transforms = Selection.transforms;

        var mask = this.mask ?? Vector3.one;
        var original = mask;

        // snapping
        if (e.control)
        {
            var snap = EditorSnapSettings.move;
            delta = Snapping.Snap(delta, snap);
        }

        // keyboard input
        if (input != "")
        {
            delta = Vector3.one * float.Parse(input);
            mask = this.mask ?? Vector3.right;
        }

        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            
            // local
            if (Tools.pivotRotation == PivotRotation.Local && t == Selection.activeTransform)
                mask = initialRotation[i] * mask;

            if (Mathf.Approximately(mask.magnitude, 1f))
            {
                var c = Camera.current.transform.position;
                Debug.Log(mask);
                var normal = Vector3.ProjectOnPlane(c - point, mask).normalized;
                Intersect(normal, true);
            }
            else if (original == Vector3.one)
            {
                t.position = initial[i] + Vector3.Scale(delta, original);
            }
            else
            {
                List<Vector3> normals = new List<Vector3>(2);
                for (int j = 0; j < 3; j++)
                {
                    if (original[j] == 1f)
                        normals.Add(activeOrientation[j]);
                }
                Intersect(Vector3.Cross(normals[0], normals[1]), false);
            }

            void Intersect(Vector3 normal, bool single)
            {
                // Debug.Log(new { normal, single});
                
                var plane = new Plane(normal, point);
                var ray = HandleUtility.GUIPointToWorldRay(mouseDelta + startMouse);
                float distance;
                plane.Raycast(ray, out distance);
                var p = ray.GetPoint(distance);

                ray = HandleUtility.GUIPointToWorldRay(startMouse);
                plane.Raycast(ray, out distance);
                var s = ray.GetPoint(distance);

                Vector3 delta;

                if(t == Selection.activeTransform)
                    delta = p - s;
                else
                    delta = initialRotation[i] * (p - s);

                this.gizmos.delta = delta;

                if (single)
                    t.position = initial[i] + Vector3.Dot(delta, mask) * mask;
                else
                    t.position = initial[i] + delta;
            }
        }

    }

    public override void Perform()
    {
        base.Perform();

        var transforms = Selection.transforms;
        for (int i = 0; i < transforms.Length; i++)
        {
            var now = transforms[i].position;
            transforms[i].position = initial[i];
            Undo.RecordObject(transforms[i], "Move");
            transforms[i].position = now;
        }

        Undo.FlushUndoRecordObjects();
    }

    public override void Cancel()
    {
        base.Cancel();

        var transforms = Selection.transforms;
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].position = initial[i];
        }
    }
}