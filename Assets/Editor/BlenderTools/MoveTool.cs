using System;
using UnityEditor;
using UnityEngine;
using static EditorState;
using static EditorHelpers;

[InitializeOnLoad]
public class MoveTool : TransformTool
{
    public override Predicate<Event> trigger => e => TriggerOn(e, KeyCode.G);

    static MoveTool()
    {
        tools.Add(new MoveTool());
    }

    public override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        var m = this.mask ?? Vector3.one;
        if (local)
        {
            m = active.rotation * m;
        }

        switch (mode)
        {
            case MoveMode.All:
                for (int j = 0; j < transforms.Length; j++)
                {
                    var t = transforms[j];
                    t.position = initialPosition[j] + Snap(delta);
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
                    m = (Vector3)mask;
                    if (local)
                    {
                        m = initialRotation[j] * m;
                    }

                    var t = transforms[j];
                    t.position = initialPosition[j] + m * Snap(axisDelta);
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
                    t.position = initialPosition[j] + t.rotation * Snap(planeDelta);
                }
                
                break;
        }
    }

    Vector3 Plane(Vector3 normal, Vector2 mouse)
    {
        var ray = HandleUtility.GUIPointToWorldRay(mouse);
        var plane = new Plane(normal, point);
        plane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }

    Vector3 Snap(Vector3 point) {
        if(!snap) return point;
        var moveSnap = EditorSnapSettings.move;
        point = Snapping.Snap(point, moveSnap);
        return point;
    }

    float Snap(float point) {
        if(!snap) return point;
        Vector3 m = EditorSnapSettings.move;
        
        var moveSnap = m.x;
        if(mask == Vector3.up) moveSnap = m.y;
        if(mask == Vector3.forward) moveSnap = m.z;

        point = Snapping.Snap(point, moveSnap);
        return point;
    }

    public override void Numerical(float input)
    {
        for (int j = 0; j < transforms.Length; j++)
        {
            var m = this.mask ?? Vector3.right;
            if(local) {
                m = initialRotation[j] * m;
            }
            var t = transforms[j];
            t.position = initialPosition[j] + m * input;
        }
    }
}