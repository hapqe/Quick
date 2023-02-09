using System;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;
using Gizmos = TransformToolGizmos;

class ScaleTool : TransformTool<ScaleTool>
{
    Vector2 startDir;

    Vector3[] initialSigns;

    float sign;

    float multiplier;

    internal override void Start() {
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

    internal override void Update(SceneView sceneView)
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

    internal override void Numerical(float input)
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

    [MenuItem("BlenderTools/Transform/Scale Tool")]
    static void Use() => MakeActive();
}