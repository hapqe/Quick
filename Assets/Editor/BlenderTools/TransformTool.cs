using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;
using Gizmos = TransformToolGizmos;

public enum MoveMode
{
    All,
    Plane,
    Axis
}

public abstract class TransformTool : IStateTool
{
    const float preciseFactor = 0.1f;

    public abstract Predicate<Event> trigger { get; }

    protected Vector3 point;
    protected Vector3 start;

    private Vector3 delta;

    protected Vector3[] directions;
    protected Vector3? mask;
    protected string input = "";

    protected bool inputValid { get => float.TryParse(input, out _); }

    protected Vector3[] activeOrientation;
    protected Vector3[] initialPosition;
    protected Quaternion[] initialRotation;
    protected Vector3[] initialScale;
    protected Vector3[] initialDirections;
    protected Vector3 planeNormal;

    bool mmb;

    protected Vector2 startMouse;
    protected Vector2 startTransformMouse;
    protected Vector2 mouseDelta;
    protected Vector2 lastMouse;

    protected MoveMode mode;
    protected bool swap;
    protected bool snap;
    protected bool local;

    protected Transform[] transforms;
    protected Transform active;

    protected float precise => Event.current.shift ? preciseFactor : 1;

    public virtual void Start()
    {
        active = Selection.activeTransform;
        transforms = Selection.transforms;

        initialPosition = new Vector3[transforms.Length];
        initialRotation = new Quaternion[transforms.Length];
        initialScale = new Vector3[transforms.Length];
        var dirs = new List<Vector3>();
        for (int i = 0; i < transforms.Length; i++)
        {
            initialPosition[i] = transforms[i].transform.position;
            initialRotation[i] = transforms[i].transform.rotation;
            initialScale[i] = transforms[i].transform.localScale;
            dirs.AddRange(new Vector3[] {
                transforms[i].right,
                transforms[i].up,
                transforms[i].forward
            });
        }
        initialDirections = dirs.ToArray();

        median = Median(transforms);

        point = pivot ?? median;

        start = GetPlanePosition(point, Event.current.mousePosition);

        Gizmos.showAll = false;

        mask = null;

        Gizmos.point = point;
        Gizmos.points = initialPosition;

        activeOrientation = new Vector3[] {
            Selection.activeTransform.right,
            Selection.activeTransform.up,
            Selection.activeTransform.forward
        };

        input = "";

        startTransformMouse = HandleUtility.WorldToGUIPoint(point);
        startMouse = Event.current.mousePosition;
        mouseDelta = Vector2.zero;
        lastMouse = startMouse - startTransformMouse;

        mode = MoveMode.All;
        swap = false;
    }

    public virtual void Update(SceneView sceneView)
    {
        var e = Event.current;

        Continuous(sceneView, e);

        delta = GetPlanePosition(point, startMouse + mouseDelta) - start;
        Gizmos.delta = delta;

        snap = e.control;

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

                mask = GetMask(e, delta, point, activeOrientation, out mode, out planeNormal);

                Gizmos.mask = mask;
            }

        var screen = LogicalRect(Camera.current.pixelRect);
        if (Math.Abs(e.delta.x) < screen.width - 50 && Math.Abs(e.delta.y) < screen.height - 50)
            mouseDelta += e.delta;

        Letters();
        AppendEvent(e, ref input);

        Gizmos.Draw();
    }

    public void AfterUpdate()
    {
        if(float.TryParse(input, out var value))
            Numerical(value);

        lastMouse = startMouse - startTransformMouse + mouseDelta;
    }

    private static void Continuous(SceneView sceneView, Event e)
    {
        var rect = PhysicalRect(sceneView.position);
        var screen = Camera.current.pixelRect;

        var m = PhysicalPoint(e.mousePosition);

        var navHeight = (int)(rect.height - screen.height);
        var margin = 10;
        var innerMargin = 20;

        var x = (int)(m.x + rect.min.x);
        if (m.y < margin)
        {
            Rust.set_cursor_pos(x, (int)rect.max.y);
        }
        if (m.y > screen.height - margin)
        {
            Rust.set_cursor_pos(x, (int)rect.min.y + navHeight + innerMargin * 2);
        }

        var y = (int)(m.y + rect.min.y + navHeight);
        if (m.x < margin)
        {
            Rust.set_cursor_pos((int)rect.max.x - innerMargin, y);
        }
        if (m.x > screen.width - margin)
        {
            Rust.set_cursor_pos((int)rect.min.x + innerMargin, y);
        }
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

    public virtual void Perform()
    {
        Gizmos.show = false;

        var transforms = Selection.transforms;
        for (int i = 0; i < transforms.Length; i++)
        {
            var newPosition = transforms[i].position;
            var newRotation = transforms[i].rotation;
            var newScale = transforms[i].localScale;
            transforms[i].position = initialPosition[i];
            transforms[i].rotation = initialRotation[i];
            transforms[i].localScale = initialScale[i];
            Undo.RecordObject(transforms[i], "Transform");
            transforms[i].position = newPosition;
            transforms[i].rotation = newRotation;
            transforms[i].localScale = newScale;
        }

        Undo.FlushUndoRecordObjects();

        Gizmos.showMouse = false;
    }

    public virtual void Cancel()
    {
        Gizmos.show = false;

        var transforms = Selection.transforms;
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].position = initialPosition[i];
            transforms[i].rotation = initialRotation[i];
            transforms[i].localScale = initialScale[i];
        }

        Gizmos.showMouse = false;
    }

    public void Letters()
    {
        LimitDirection(KeyCode.X, Vector3.right);
        LimitDirection(GetKey(KeyCode.Y), Vector3.up);
        LimitDirection(GetKey(KeyCode.Z), Vector3.forward);

        Gizmos.show = mask != null;
        Gizmos.mask = mask;
        Directions(Selection.transforms, Tools.pivotRotation == PivotRotation.Global ^ swap);
    }

    private void LimitDirection(KeyCode code, Vector3 direction)
    {
        if (Key(code))
        {
            var s = Event.current.shift;
            var now = s ? MoveMode.Plane : MoveMode.Axis;
            if (now != mode)
            {
                swap = false;
                mask = null;
            }
            mode = now;

            if (mask == direction || mode == MoveMode.Plane && mask == Vector3.one - direction)
            {
                if (!swap)
                    swap = true;
                else
                {
                    mask = null;
                    mode = MoveMode.All;
                }
            }
            else
            {
                mask = direction;
                swap = false;
            }

            if (mode == MoveMode.Plane)
            {
                planeNormal = direction;
                mask = Vector3.one - direction;
            }
        }
    }

    protected Vector3 median;

    protected Vector3? pivot {
        get {
            switch (PivotDropdown.pivotMode)
            {
                case PivotMode.Median:
                    var mean = Vector3.zero;
                    foreach (var p in initialPosition)
                        mean += p;
                    return mean / transforms.Length;
                case PivotMode.Cursor:
                    return Cursor.position;
                default:
                    return null;
            }
        }
    }

    public abstract Action triggerAgain { get; }

    public abstract void Numerical(float input);
}