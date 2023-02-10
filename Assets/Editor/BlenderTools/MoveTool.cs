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

        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            var start = HandleUtility.WorldToGUIPoint(initial[i].position);
            var activeStart = HandleUtility.WorldToGUIPoint(active.position);

            switch (mode)
            {
                case TransformMode.All:
                    t.position = Plane(Camera.current.transform.forward, delta + start, initial[i].position);
                    break;
                case TransformMode.Plane:
                    var axis = Vector3.one - mask;
                    if (global)
                        t.position = Plane(axis, delta + start, initial[i].position);
                    else
                    {
                        var pos = Plane(active.rotation * axis, delta + activeStart, active.position);
                        var offset = pos - active.position;
                        offset = Quaternion.Inverse(active.rotation) * offset;
                        offset = initial[i].rotation * offset;
                        t.position = initial[i].position + offset;
                    }
                    break;
                case TransformMode.Axis:
                    var cameraPos = Camera.current.transform.position;
                    axis = mask;
                    if (local) axis = active.rotation * axis;

                    axis = Vector3.ProjectOnPlane(cameraPos - active.position, axis);
                    var p = Plane(axis, delta + activeStart, active.position);

                    if (global)
                    {
                        var offset = p - active.position;
                        t.position = initial[i].position + Vector3.Scale(offset, mask);
                    }
                    else
                    {
                        var offset = p - active.position;
                        offset = Quaternion.Inverse(active.rotation) * offset;
                        offset = initial[i].rotation * offset;
                        t.position = initial[i].position + Vector3.Project(offset, initial[i].rotation * mask);
                    }
                    break;
            }
        }
    }

    Vector3 Plane(Vector3 normal, Vector2 mouse, Vector3 point)
    {
        var ray = HandleUtility.GUIPointToWorldRay(mouse);
        var plane = new Plane(normal, point);
        plane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }

    internal override void Numerical(float input)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            var m = this.mode == TransformMode.All ? Vector3.right : mask;
            if (local)
            {
                m = initial[i].rotation * m;
            }
            var t = transforms[i];
            t.position = initial[i].position + m * input;
        }
    }

    const string menuName = "BlenderTools/Transform/Move _g";
    [MenuItem(menuName)]
    static void Use() => MakeActive(menuName);
}