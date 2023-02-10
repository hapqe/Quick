using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;
using Gizmos = TransformToolGizmos;

public enum TransformMode
{
    All,
    Plane,
    Axis
}

internal struct TransformProperties
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformProperties(Transform t)
    {
        position = t.position;
        rotation = t.rotation;
        scale = t.localScale;
    }

    public void Apply(Transform t)
    {
        t.position = position;
        t.rotation = rotation;
        t.localScale = scale;
    }
}

abstract class TransformTool<T> : StateTool<T> where T : TransformTool<T>
{
    protected Vector3 point { get; private set; }

    protected Vector3[] directions { get; private set; }
    protected Vector3 mask { get; private set; }

    protected Vector3[] orientation { get; private set; }
    protected TransformProperties[] initial { get; private set; }
    protected Vector3[] initialDirections { get; private set; }
    new protected TransformProperties active { get; private set; }

    bool mmb;

    protected TransformMode mode { get; private set; }
    protected bool swap { get; private set; }
    protected bool snapping { get; private set; }
    protected bool local { get; private set; }
    protected Vector3 median { get; private set; }

    protected bool global => !local;

    protected Transform[] transforms;

    internal override bool Requirements()
    {
        return Selection.activeTransform != null && Selection.transforms.Length > 0;
    }

    internal override void Start()
    {
        base.Start();
        
        active = new TransformProperties(Selection.activeTransform);
        transforms = Selection.transforms;

        initial = new TransformProperties[transforms.Length];
        var dirs = new List<Vector3>();
        for (int i = 0; i < transforms.Length; i++)
        {
            initial[i] = new TransformProperties(transforms[i]);
            dirs.AddRange(TransformDirs(transforms[i]));
        }
        initialDirections = dirs.ToArray();

        median = Median(transforms);

        point = pivot ?? median;

        Gizmos.showAll = false;

        mask = Vector3.one;

        Gizmos.point = point;
        Gizmos.points = transforms.Select(t => t.position).ToArray();

        orientation = TransformDirs(Selection.activeTransform);

        mode = TransformMode.All;
        swap = false;
    }

    internal override void Update(SceneView sceneView)
    {
        base.Update(sceneView);
        
        var e = Event.current;

        Gizmos.delta = absoluteDelta;
        Gizmos.pivot = pivot;
        Gizmos.mouse = mouse;

        local = Tools.pivotRotation == PivotRotation.Local;
        local = swap ^ local;

        // mmb down
        if (e.type == EventType.MouseDown && e.button == 2)
        {
            mmb = true;
            swap = false;
            e.Use();
        }

        // mmb up
        if (e.type == EventType.MouseUp && e.button == 2)
        {
            mmb = false;
            e.Use();
        }

        Gizmos.showAll = mmb;

        if (e.type != EventType.Layout || e.type != EventType.Repaint)
            if (mmb)
            {
                Gizmos.show = true;
                Gizmos.showAll = true;

                var global = Tools.pivotRotation == PivotRotation.Global;

                Directions(transforms, global);

                mask = GetMask(e, absoluteDelta, point, orientation, out var m);
                mode = m;
            }

        Axes();
        
        Gizmos.show = mask != Vector3.one;
        Gizmos.mask = mask;
        Gizmos.Draw();
    }

    private void Directions(Transform[] transforms, bool global)
    {
        directions = new Vector3[transforms.Length * 3];
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];

            if (global)
            {
                directions[i * 3 + 0] = Vector3.right;
                directions[i * 3 + 1] = Vector3.up;
                directions[i * 3 + 2] = Vector3.forward;
            }
            else
            {
                directions[i * 3 + 0] = initialDirections[i * 3 + 0];
                directions[i * 3 + 1] = initialDirections[i * 3 + 1];
                directions[i * 3 + 2] = initialDirections[i * 3 + 2];
            }
        }

        Gizmos.directions = directions;
    }

    internal override void Perform()
    {
        Gizmos.show = false;

        var transforms = Selection.transforms;
        for (int i = 0; i < transforms.Length; i++)
        {
            var now = new TransformProperties(transforms[i]);
            initial[i].Apply(transforms[i]);
            Undo.RecordObject(transforms[i], "Transform");
            now.Apply(transforms[i]);
        }

        Undo.FlushUndoRecordObjects();

        Gizmos.showMouse = false;
    }

    internal override void Cancel()
    {
        Gizmos.show = false;

        var transforms = Selection.transforms;
        for (int i = 0; i < transforms.Length; i++)
        {
            initial[i].Apply(transforms[i]);
        }

        Gizmos.showMouse = false;
    }

    public void Axes()
    {
        LimitDirection(KeyCode.X, Vector3.right);
        LimitDirection(GetKey(KeyCode.Y), Vector3.up);
        LimitDirection(GetKey(KeyCode.Z), Vector3.forward);

        Directions(Selection.transforms, Tools.pivotRotation == PivotRotation.Global ^ swap);
    }

    private void LimitDirection(KeyCode code, Vector3 direction)
    {
        if(!Key(code)) return;

        var s = Event.current.shift;
        var now = s ? TransformMode.Plane : TransformMode.Axis;
        if (now != mode)
        {
            swap = false;
            mask = Vector3.one;
        }
        mode = now;

        if (mask == direction || mode == TransformMode.Plane && mask == Vector3.one - direction)
        {
            if (!swap)
                swap = true;
            else
            {
                mask = Vector3.one;
                mode = TransformMode.All;
            }
        }
        else
        {
            mask = direction;
            swap = false;
        }

        if (mode == TransformMode.Plane)
        {
            mask = Vector3.one - direction;
        }
    }

    protected Vector3? pivot {
        get {
            switch (Pivot.mode)
            {
                case PivotMode.Median:
                    var mean = Vector3.zero;
                    foreach (var p in initial.Select(i => i.position))
                        mean += p;
                    return mean / transforms.Length;
                case PivotMode.Cursor:
                    return Cursor.position;
                default:
                    return null;
            }
        }
    }
}