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
    public override Action triggerAgain => () => gimbal = !gimbal;

    float angle;
    bool gimbal;
    Vector2 delta;

    (Vector3, Vector3) gimbalAxes;

    static RotateTool()
    {
        tools.Add(new RotateTool());
    }

    public override void Start()
    {
        base.Start();

        Gizmos.showMouse = true;

        angle = 0;
        delta = Vector2.zero;
        gimbal = false;

        var normal = Camera.current.transform.forward;
        var plane = new Plane(normal, point);
        var ray1 = HandleUtility.GUIPointToWorldRay(startTransformMouse + Vector2.right);
        var ray2 = HandleUtility.GUIPointToWorldRay(startTransformMouse + Vector2.up);

        var p1 = Vector3.zero;
        var p2 = Vector3.zero;

        if(plane.Raycast(ray1, out var d1))
            p1 = ray1.GetPoint(d1);

        if(plane.Raycast(ray2, out var d2))
            p2 = ray2.GetPoint(d2);

        gimbalAxes = (p1 - point, p2 - point);
    }

    public override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        // r
        if (Input.GetKeyDown(KeyCode.R))
            gimbal = !gimbal;

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
        delta += (startMouse - startTransformMouse + mouseDelta - lastMouse) * precise;

        var a = angle;
        if(float.TryParse(input, out var n))
            a = -n;

        a = Snap(a);

        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];

            t.rotation = initialRotation[i];
            t.position = initialPosition[i];

            if(!gimbal) {
                if (local && mode != MoveMode.All){
                    axis = initialRotation[i] * m;
                    if(transforms[i] == active)
                        axis *= Vector3.Dot(fwd, axis) > 0 ? 1 : -1;
                }
                if (pivot != null)
                {
                    t.RotateAround((Vector3)pivot, axis, a);
                }
                else
                {
                    t.Rotate(axis, a);
                }
            }
            else {
                var (x, y) = gimbalAxes;
                if (local && mode != MoveMode.All){
                    x = initialRotation[i] * x;
                    y = initialRotation[i] * y;
                    if(transforms[i] == active) {
                        x *= Vector3.Dot(fwd, x) > 0 ? 1 : -1;
                        y *= Vector3.Dot(fwd, y) > 0 ? 1 : -1;
                    }
                }
                if (pivot != null)
                {
                    t.RotateAround((Vector3)pivot, x, -delta.y);
                    t.RotateAround((Vector3)pivot, y, delta.x);
                }
                else
                {
                    t.Rotate(x, -delta.y, Space.World);
                    t.Rotate(y, delta.x, Space.World);
                }
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