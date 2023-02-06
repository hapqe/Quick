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

        var local = Tools.pivotRotation == PivotRotation.Local;
        local = swap ^ local;

        var transforms = Selection.transforms;
        var mask = this.mask ?? Vector3.one;
        var active = Selection.activeTransform;

        var m = this.mask ?? Vector3.one;
        if (local)
        {
            m = active.rotation * m;
        }

        // local

        switch (mode)
        {
            case MoveMode.All:
                for (int j = 0; j < transforms.Length; j++)
                {
                    var t = transforms[j];
                    t.position = initial[j] + delta;
                }
                break;
            case MoveMode.Axis:
                var c = Camera.current.transform.position;
                var n = Vector3.ProjectOnPlane(c - point, m).normalized;
                var i = Plane(n, startTransformMouse);

                var now = Plane(n, startTransformMouse + mouseDelta);

                var axisDelta = Vector3.Dot(now - i, m);

                for (int j = 0; j < transforms.Length; j++)
                {
                    m = mask;
                    if (local)
                    {
                        m = initialRotation[j] * m;
                    }

                    var t = transforms[j];
                    t.position = initial[j] + m * axisDelta;
                }
                break;
            case MoveMode.Plane:
                n = planeNormal;
                if (local)
                {
                    n = active.rotation * n;
                }
                
                i = Plane(n, startTransformMouse);

                now = Plane(n, startTransformMouse + mouseDelta);

                var planeDelta = now - i;
                planeDelta = Quaternion.Inverse(active.rotation) * planeDelta;

                for (int j = 0; j < transforms.Length; j++)
                {
                    var t = transforms[j];
                    t.position = initial[j] + t.rotation * planeDelta;
                }
                
                break;
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

    Vector3 Plane(Vector3 normal, Vector2 mouse)
    {
        var ray = HandleUtility.GUIPointToWorldRay(mouse);
        var plane = new Plane(normal, point);
        plane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }
}