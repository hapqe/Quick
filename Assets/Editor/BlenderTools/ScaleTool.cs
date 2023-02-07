using System;
using UnityEditor;
using UnityEngine;
using static EditorState;
using static EditorHelpers;

[InitializeOnLoad]
public class ScaleTool : TransformTool
{
    public override Predicate<Event> trigger => e => TriggerOn(e, KeyCode.S);

    Vector2 startDir;

    float sign;

    static ScaleTool()
    {
        tools.Add(new ScaleTool());
    }

    public override void Start() {
        base.Start();

        startDir = startMouse - startTransformMouse;
    }

    public override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        var m = this.mask ?? Vector3.one;

        var original = (startMouse - startTransformMouse).magnitude;
        var diff = startMouse - startTransformMouse + mouseDelta;
        sign = Mathf.Sign(Vector3.Dot(diff, startDir));

        var multiplier = sign * diff.magnitude / original;

        for (int i = 0; i < transforms.Length; i++)
        {
            ScaleTransform(multiplier, i);
        }
    }

    void ScaleTransform(float scale, int i)
    {
        var t = transforms[i];
        Vector3 initial = initialScale[i];
        
        var m = this.mask ?? Vector3.one;
        var inverse = m;
        
        Vector3 diff;
        
        if (local || mode == MoveMode.All)
        {
            diff = initial * scale - initial;
            diff = Vector3.Scale(diff, m);
            t.localScale = initial + diff;
            inverse = t.rotation * inverse;
        }
        else
        {
            m = initialRotation[i] * m;
            m = AbsVector(m);
            diff = initial * Mathf.Abs(scale) - initial;
            diff = Vector3.Scale(diff, m);
            t.localScale = initial + diff;
            t.localScale = Vector3.Scale(t.localScale, SignVector((Vector3)mask, sign));
        }

        OffsetTransform(t, initialPosition[i], scale, inverse);
    }

    void OffsetTransform(Transform t, Vector3 initial, float scale, Vector3 mask)
    {
        if(pivot == null) {
            t.position = initial;
            return;
        }

        t.position = ScalePointAbout(initial, (Vector3)pivot, scale, mask);
    }

    public override void Numerical(float input)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            ScaleTransform(input, i);
        }
    }

    public float Snap(float input)
    {
        if(!snap) return input;
        var s = EditorSnapSettings.scale;
        return Snapping.Snap(input, s);
    }
}