using System;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

class MoveTool : TransformTool<MoveTool>
{
    Vector2 scaledMouseDelta;

    internal override void Start()
    {
        base.Start();

        scaledMouseDelta = Vector2.zero;
    }

    internal override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        var m = this.mask ?? Vector3.one;
        if (local)
        {
            m = active.rotation * m;
        }

        var mouse = startMouse - startTransformMouse + mouseDelta;
        scaledMouseDelta += (mouse - lastMouse) * precise;

        switch (mode)
        {
            case MoveMode.All:
                for (int j = 0; j < transforms.Length; j++)
                {
                    var t = transforms[j];
                    var delta = GetPlanePosition(point, startMouse + scaledMouseDelta) - start;
                    t.position = initialPosition[j] + Snap(delta);
                }
                break;
            case MoveMode.Axis:
                var c = Camera.current.transform.position;
                var n = Vector3.ProjectOnPlane(c - point, m).normalized;
                var i = Plane(n, startTransformMouse);

                var now = Plane(n, startTransformMouse + scaledMouseDelta);

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

                now = Plane(n, startTransformMouse + scaledMouseDelta);

                var planeDelta = now - i;
                planeDelta = Quaternion.Inverse(active.rotation) * planeDelta;

                for (int j = 0; j < transforms.Length; j++)
                {
                    var t = transforms[j];
                    t.position = initialPosition[j] + Snap(planeDelta);
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
        point = Snapping.Snap(point, moveSnap * precise);
        return point;
    }

    float Snap(float point) {
        if(!snap) return point;
        Vector3 m = EditorSnapSettings.move;
        
        var moveSnap = m.x;
        if(mask == Vector3.up) moveSnap = m.y;
        if(mask == Vector3.forward) moveSnap = m.z;

        point = Snapping.Snap(point, moveSnap * precise);
        return point;
    }

    internal override void Numerical(float input)
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

    [MenuItem("BlenderTools/Transform/Move Tool")]
    static void Use() => MakeActive();
}