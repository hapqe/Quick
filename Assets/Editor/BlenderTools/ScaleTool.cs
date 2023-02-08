using System;
using UnityEditor;
using UnityEngine;
using static EditorState;
using static EditorHelpers;
using Gizmos = TransformToolGizmos;

[InitializeOnLoad]
public class ScaleTool : TransformTool
{
    public override Predicate<Event> trigger => e => TriggerOn(e, KeyCode.S);
    public override Action triggerAgain => () => {};

    Vector2 startDir;

    Vector3[] initialSigns;

    float sign;

    float multiplier;
    static ScaleTool()
    {
        tools.Add(new ScaleTool());
    }

    public override void Start() {
        base.Start();

        startDir = startMouse - startTransformMouse;
        Gizmos.showMouse = true;

        multiplier = 1;

        initialSigns = new Vector3[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            initialSigns[i] = SignVector(initialScale[i]);
        }
    }

    public override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        var m = this.mask ?? Vector3.one;

        var original = (startMouse - startTransformMouse).magnitude;
        var absDiff = startMouse - startTransformMouse + mouseDelta;
        sign = Mathf.Sign(Vector3.Dot(absDiff, startDir));

        if(float.TryParse(input, out var x)) {
            sign *= Mathf.Sign(x);
        }
        

        var mouse = startMouse - startTransformMouse + mouseDelta;
        var lastMultiplier = multiplier;
        multiplier *= sign * (mouse.magnitude / lastMouse.magnitude);
        var diff = multiplier - lastMultiplier;
        multiplier = lastMultiplier + diff * precise;

        for (int i = 0; i < transforms.Length; i++)
        {
            ScaleTransform(Snap(multiplier), i);
        }
    }

    void ScaleTransform(float scale, int i)
    {
        var t = transforms[i];
        Vector3 initial = initialScale[i];
        
        var m = this.mask ?? Vector3.one;
        var inverse = m;
        
        Vector3 diff;
        
        if (local || mask == null)
        {
            diff = initial * scale - initial;
            diff = Vector3.Scale(diff, m);
            t.localScale = initial + diff;
            inverse = t.rotation * inverse;
            t.localScale = AbsVector(t.localScale);
            var signed = Vector3.Scale(initialSigns[i], SignVector(m, sign));
            t.localScale = Vector3.Scale(t.localScale, signed);
        }
        else
        {
            m = initialRotation[i] * m;
            m = AbsVector(m);
            diff = initial * Mathf.Abs(scale) - initial;
            diff = Vector3.Scale(diff, m);
            t.localScale = initial + diff;
            var signed = Vector3.Scale(initialSigns[i], SignVector(mask ?? Vector3.one, sign));
            t.localScale = Vector3.Scale(t.localScale, signed);
        }

        if(mask == null) inverse = Vector3.one;
        OffsetTransform(t, initialPosition[i], Mathf.Abs(scale) * sign, inverse);
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
        var s = EditorSnapSettings.scale * precise;
        return Snapping.Snap(input, s);
    }
}