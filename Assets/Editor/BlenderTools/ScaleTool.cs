using System;
using UnityEditor;
using UnityEngine;
using static EditorState;
using static EditorHelpers;

[InitializeOnLoad]
public class ScaleTool : TransformTool
{
    public override Predicate<Event> trigger => e => TriggerOn(e, KeyCode.S);

    static ScaleTool()
    {
        tools.Add(new ScaleTool());
    }

    public override void Update(SceneView sceneView)
    {
        base.Update(sceneView);

        var m = this.mask ?? Vector3.one;

        for (int j = 0; j < transforms.Length; j++)
        {
            var t = transforms[j];
            var i = initialScale[j];

            var original = (startMouse - startTransformMouse).magnitude;
            var multiplier = (startMouse - startTransformMouse + mouseDelta).magnitude / original;

            ScaleTransform(t, multiplier, i, initialPosition[j]);
        }
    }

    void ScaleTransform(Transform t, float scale, Vector3 initialScale, Vector3 initialPosition)
    {
        scale = Snap(scale);
        
        var m = this.mask ?? Vector3.one;
        var om = m;
        
        Vector3 diff;
        
        if (local || mode == MoveMode.All)
        {
            diff = initialScale * scale - initialScale;
            diff = Vector3.Scale(diff, m);
            t.localScale = initialScale + diff;
            om = t.rotation * om;
        }
        else
        {
            var s = initialScale * scale - initialScale;
            m = t.rotation * m;
            m = AbsVector(m);
            diff = initialScale * scale - initialScale;
            diff = Vector3.Scale(diff, m);
            t.localScale = initialScale + diff;
        }

        OffsetTransform(t, initialPosition, scale, om);
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
        for (int j = 0; j < transforms.Length; j++)
        {
            var t = transforms[j];
            var i = initialScale[j];

            ScaleTransform(t, input, i, initialPosition[j]);
        }
    }

    public float Snap(float input)
    {
        if(!snap) return input;
        var s = EditorSnapSettings.scale;
        return Snapping.Snap(input, s);
    }
}