using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

public class TransformTool : IStateTool
{
    public virtual Predicate<Event> trigger => throw new NotImplementedException();

    protected Vector3[] initial;
    protected Vector3 point;
    protected Vector3 start;

    protected Vector3 delta;

    protected Vector3[] directions;
    protected Vector3? mask;
    protected string input = "";

    protected Vector3 activePosition;
    protected Vector3[] activeOrientation;

    protected Quaternion[] initialRotation;

    bool mmb;

    protected TransformGizmos gizmos {get => EditorHelpers.GetDrawer<TransformGizmos>(); }

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

        start = GetPlanePosition(point);

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
    }

    public virtual void Update(SceneView sceneView)
    {
        var e = Event.current;

        var transforms = Selection.transforms;
        
        delta = GetPlanePosition(point) - start;
        gizmos.delta = delta;

        // mmb down
        if(e.type == EventType.MouseDown && e.button == 2) {
            mmb = true;
            e.Use();
        }

        // mmb up
        if(e.type == EventType.MouseUp && e.button == 2) {
            mmb = false;
            e.Use();
        }

        gizmos.showAll = mmb;

        if(e.type != EventType.Layout || e.type != EventType.Repaint)
        if(mmb) {
            gizmos.show = true;
            gizmos.showAll = true;
            
            var global = Tools.pivotRotation == PivotRotation.Global;
            
            directions = new Vector3[transforms.Length * 3];
            for (int i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];

                if(global) {
                    directions[i * 3 + 0] = Vector3.right;
                    directions[i * 3 + 1] = Vector3.up;
                    directions[i * 3 + 2] = Vector3.forward;
                }
                else {
                    directions[i * 3 + 0] = t.right;
                    directions[i * 3 + 1] = t.up;
                    directions[i * 3 + 2] = t.forward;
                }
            }

            gizmos.directions = directions;

            mask = GetMask(e, delta, activePosition, activeOrientation);

            gizmos.mask = mask;
        }

        AppendEvent(e, ref input);
    }

    public virtual void Perform()
    {
        gizmos.show = false;
    }

    public virtual void Cancel()
    {
        gizmos.show = false;
    }
}