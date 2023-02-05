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

        for (int i = 0; i < transforms.Length; i++)
        {
            var mask = this.mask??Vector3.one;

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

            // local
            if (Tools.pivotRotation == PivotRotation.Local)
                mask = initialRotation[i] * mask;
                
            if(Mathf.Approximately(mask.magnitude, 1f))
            {
                var c = Camera.current.transform.position;
                var normal = Vector3.ProjectOnPlane(c - activePosition, mask).normalized;
                Intersect(normal);
            }
            else if(mask == Vector3.one) {
                var t = transforms[i];
                t.position = initial[i] + Vector3.Scale(delta, mask);
            }
            else {
                List<Vector3> normals = new List<Vector3>(2);
                for (int j = 0; j < 3; j++)
                {
                    if(mask[j] == 1f)
                        normals.Add(activeOrientation[j]);
                }
                Intersect(Vector3.Cross(normals[0], normals[1]));
            }

            void Intersect(Vector3 normal)
            {
                var plane = new Plane(normal, start);
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                float distance;
                plane.Raycast(ray, out distance);

                var p = ray.GetPoint(distance);
                distance = Vector3.Dot(p, mask);
                var delta = p - start;

                this.gizmos.delta = delta;

                var t = transforms[i];
                t.position = initial[i] + Vector3.Scale(delta, mask);
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