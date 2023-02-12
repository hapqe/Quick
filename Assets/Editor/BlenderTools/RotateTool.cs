using System;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;
using Gizmos = TransformToolGizmos;

class RotateTool : TransformTool<RotateTool>
{
    bool trackball;
    float angle;
    Vector3 trackballDiff;

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
                var normal = Camera.current.transform.forward;
                var plane = new Plane(normal, point);
                var ray = HandleUtility.GUIPointToWorldRay(start + currentDelta);
                if (plane.Raycast(ray, out var distance))
                {
                    var p = ray.GetPoint(distance);
                    var orth = p - point;
                    var dir = Vector3.Cross(orth, normal);
                    if(local && pivot == null) {
                        dir = Quaternion.Inverse(active.rotation) * dir;
                        dir = initial[i].rotation * dir;
                    }
                    t.rotation = Quaternion.Euler(trackballDiff) * initial[i].rotation;
                    t.RotateAround(pivot ?? initial[i].position, dir, currentDelta.magnitude);
                    trackballDiff = (t.rotation * Quaternion.Inverse(initial[i].rotation)).eulerAngles;
                    var diff = Snap(trackballDiff);
                    Debug.Log(new {diff, trackballDiff});
                    // t.rotation = initial[i].rotation * Quaternion.Euler(diff);
                    t.rotation = Quaternion.Euler(diff) * initial[i].rotation;
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