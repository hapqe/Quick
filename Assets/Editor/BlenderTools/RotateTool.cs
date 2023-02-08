using System;
using UnityEditor;
using UnityEngine;
using static EditorState;
using static EditorHelpers;
using Gizmos = TransformToolGizmos;

[InitializeOnLoad]
public class RotateTool : TransformTool
{
    public override Predicate<Event> trigger => e => TriggerOn(e, KeyCode.R);

    float angle;

    static RotateTool()
    {
        tools.Add(new RotateTool());
    }

    public override void Start()
    {
        base.Start();

        Gizmos.showMouse = true;

        angle = 0;
    }

    public override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        Vector3 axis;

        var m = this.mask ?? Vector3.one;
        var fwd = Camera.current.transform.forward;

        switch (mode)
        {
            case MoveMode.All:
                axis = fwd;
                break;
            case MoveMode.Plane:
                axis = Vector3.one - m;
                break;
            default:
                axis = m;
                break;
        }
        axis *= Vector3.Dot(fwd, axis) > 0 ? 1 : -1;

        angle -= Vector2.SignedAngle(lastMouse, startMouse - startTransformMouse + mouseDelta) * precise;

        var a = angle;
        if(float.TryParse(input, out var x))
            a = -x;

        a = Snap(a);

        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];

            if (local && mode != MoveMode.All){
                axis = initialRotation[i] * m;
                if(transforms[i] == active)
                    axis *= Vector3.Dot(fwd, axis) > 0 ? 1 : -1;
            }

            var rot = Quaternion.AngleAxis(a, axis);
            t.rotation = rot * initialRotation[i];

            if (pivot != null)
            {
                var p = (Vector3)pivot;
                t.rotation = initialRotation[i];
                t.position = initialPosition[i];
                t.RotateAround(p, axis, a);
            }
        }
    }

    public override void Numerical(float input)
    {
        angle = input;
    }

    float Snap(float input)
    {
        if(!snap) return input;
        var s = EditorSnapSettings.rotate * precise;
        return Snapping.Snap(input, s);
    }
}