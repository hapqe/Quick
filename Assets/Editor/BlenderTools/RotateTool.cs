using System;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;
using Gizmos = TransformToolGizmos;

class RotateTool : TransformTool<RotateTool>
{
    bool trackball;
    float angle;

    const float TRACKBALL_STEPS = 1e3f;

    protected override float snap => EditorSnapSettings.rotate;

    internal override void Again()
    {
        base.Again();
        trackball = !trackball;
        Gizmos.showMouse = !Gizmos.showMouse;
    }

    internal override void Start()
    {
        base.Start();

        Gizmos.showMouse = true;

        trackball = false;
    }

    Vector3 TrackballDir(Vector2 dir) {
        var fwd = Camera.current.transform.forward;
        var plane = new Plane(fwd, point);
        var start = HandleUtility.WorldToGUIPoint(point);
        var ray = HandleUtility.GUIPointToWorldRay(start + dir);
        plane.Raycast(ray, out var dist);
        return ray.GetPoint(dist) - point;
    }

    internal override void Update(SceneView sceneView)
    {
        base.Update(sceneView);
        
        var start = HandleUtility.WorldToGUIPoint(point);
        var v = this.start - start + delta;
        this.angle -= Vector2.SignedAngle(v, v + currentDelta);
        var angle = Snap(this.angle);

        var fwd = point - Camera.current.transform.position;

        Vector3 axis;
        switch (mode)
        {
            case TransformMode.All:
                axis = fwd;
                break;
            case TransformMode.Plane:
                axis = Vector3.one - mask;
                break;
            default:
                axis = mask;
                break;
        }

        for (int i = 0; i < initial.Length; i++)
        {
            var t = transforms[i];

            if (trackball)
            {
                t.rotation = initial[i].rotation;
                t.position = initial[i].position;
                var d = Snap(delta);
                var up = TrackballDir(Vector2.up);
                var left = TrackballDir(Vector2.left);
                
                for (int j = 0; j < TRACKBALL_STEPS; j++)
                {
                    t.RotateAround(point, up, d.x / TRACKBALL_STEPS);
                    t.RotateAround(point, left, d.y / TRACKBALL_STEPS);
                }
            }
            else
            {
                var l = local && mode != TransformMode.All;
                var s = Mathf.Sign(Vector3.Dot(fwd, l ? active.rotation * axis : axis));
                if(l) axis = initial[i].rotation * axis;
                var q = Quaternion.AngleAxis(angle, axis * s);
                t.rotation = q * initial[i].rotation;

                if(pivot != null) {
                    t.position = q * (initial[i].position - pivot.Value) + pivot.Value;
                }
            }
        }
    }

    internal override void Numerical(float input)
    {
        // ...
    }

    const string menuName = "BlenderTools/Transform/Rotate _r";
    [MenuItem(menuName)]
    static void Use() => MakeActive(menuName);
}