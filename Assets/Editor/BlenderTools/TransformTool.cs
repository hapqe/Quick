using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

public enum MoveMode {
    All,
    Plane,
    Axis
}

public class TransformTool : IStateTool
{
    const float preciseFactor = 0.1f;
    
    public virtual Predicate<Event> trigger => throw new NotImplementedException();

    protected Vector3[] initial;
    protected Vector3 point;
    protected Vector3 start;

    protected Vector3 delta;

    protected Vector3[] directions;
    protected Vector3? mask;
    protected string input = "";

    protected Vector3[] activeOrientation;

    protected Quaternion[] initialRotation;

    bool mmb;

    protected TransformGizmos gizmos {get => EditorHelpers.GetDrawer<TransformGizmos>(); }

    protected Vector2 startMouse;
    protected Vector2 startTransformMouse;
    protected Vector2 mouseDelta;

    protected MoveMode mode;

    protected Vector3 planeNormal;
    protected bool swap = false;

    public void Start()
    {
        var transforms = Selection.transforms;
        
        initial = new Vector3[transforms.Length];
        initialRotation = new Quaternion[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            initial[i] = transforms[i].transform.position;
            initialRotation[i] = transforms[i].transform.rotation;
        }
        point = Selection.activeTransform.position;

        start = GetPlanePosition(point, Event.current.mousePosition);

        gizmos.showAll = false;

        mask = null;

        gizmos.point = point;
        gizmos.points = initial;

        activeOrientation = new Vector3[] {
            Selection.activeTransform.right,
            Selection.activeTransform.up,
            Selection.activeTransform.forward
        };

        input = "";
        
        startTransformMouse = HandleUtility.WorldToGUIPoint(point);
        startMouse = Event.current.mousePosition;
        mouseDelta = Vector2.zero;

        mode = MoveMode.All;
        swap = false;
    }

    public virtual void Update(SceneView sceneView)
    {
        var e = Event.current;

        var rect = PhysicalRect(sceneView.position);
        var screen = Camera.current.pixelRect;

        var m = PhysicalPoint(e.mousePosition);

        if(m.y < 10) {
            Rust.set_cursor_pos((int)(m.x + rect.min.x), (int)rect.max.y);
        }
        if(m.y > screen.height - 10) {
            Rust.set_cursor_pos((int)(m.x + rect.min.x), (int)(rect.max.y - screen.height) + 50);
        }
        var y = (int)(m.y + rect.min.y + (rect.height - screen.height) * 2);
        if(m.x < 10) {
            Rust.set_cursor_pos((int)rect.max.x - 10, y);
        }
        if(m.x > screen.width - 10) {
            Rust.set_cursor_pos((int)(rect.min.x) + 20, y);
        }


        var transforms = Selection.transforms;
        
        delta = GetPlanePosition(point, startMouse + mouseDelta) - start;
        gizmos.delta = delta;

        // mmb down
        if(e.type == EventType.MouseDown && e.button == 2) {
            mmb = true;
            swap = false;
            e.Use();
        }

        // mmb up
        if(e.type == EventType.MouseUp && e.button == 2) {
            mmb = false;
            e.Use();
        }

        gizmos.showAll = mmb;

        if(e.type != EventType.Layout || e.type != EventType.Repaint)
        if(mmb)
            {
                gizmos.show = true;
                gizmos.showAll = true;

                var global = Tools.pivotRotation == PivotRotation.Global;

                Directions(transforms, global);

                mask = GetMask(e, delta, point, activeOrientation, out mode, out planeNormal);

                gizmos.mask = mask;
            }

        var precise = e.shift;

        var d = e.delta * (precise ? preciseFactor : 1);
        
        screen = LogicalRect(screen);
        if(Math.Abs(d.x) < screen.width - 50 && Math.Abs(d.y) < screen.height - 50)
        mouseDelta += d;

        Letters();
        AppendEvent(e, ref input);
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
                directions[i * 3 + 0] = t.right;
                directions[i * 3 + 1] = t.up;
                directions[i * 3 + 2] = t.forward;
            }
        }

        gizmos.directions = directions;
    }

    public virtual void Perform()
    {
        gizmos.show = false;
    }

    public virtual void Cancel()
    {
        gizmos.show = false;
    }

    public void Letters()
    {
        LimitDirection(KeyCode.X, Vector3.right);
        LimitDirection(GetKey(KeyCode.Y), Vector3.up);
        LimitDirection(GetKey(KeyCode.Z), Vector3.forward);

        gizmos.show = mask != null;
        gizmos.mask = mask;
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

    public bool Key(KeyCode key) {
        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == key) {
            Event.current.Use();
            return true;
        }
        return false;
    }
}