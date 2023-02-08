using UnityEngine;
using UnityEditor;
using System.Linq;
using static UnityEngine.Vector3;

public static class TransformToolGizmos
{
    public static bool show;
    public static bool showAll;
    public static Vector3 delta;
    public static Vector3[] directions;
    public static Vector3? mask;
    public static Vector3 point;
    public static Vector3[] points;
    public static bool showMouse;

    public static void Draw()
    {
        if (showMouse)
        {
            var mouse = Event.current.mousePosition;
            var worldMouse = HandleUtility.GUIPointToWorldRay(mouse).origin;
            Handles.color = Color.black;
            Handles.DrawDottedLine(point, worldMouse, 2f);
        }

        if (!show) return;

        if(showAll) {
            var color = Color.white;
            color.a = .2f;
            Handles.color = color;
            Handles.DrawLine(point, point + delta);
        }

        for (int i = 0; i < directions.Length; i += 3)
        {
            var a = directions[i];
            var b = directions[i + 1];
            var c = directions[i + 2];

            var point = TransformToolGizmos.point;
            if (Pivot.mode == PivotMode.Individual)
                point = points[i / 3];

            if (showAll)
            {
                AxisRay(point, a, Color.red, .2f);
                AxisRay(point, b, Color.green, .2f);
                AxisRay(point, c, Color.blue, .2f);
            }

            if (mask != null)
            {
                var m = (Vector3)mask;
                if (m.x == 1) AxisRay(point, a, Color.red, .4f);
                if (m.y == 1) AxisRay(point, b, Color.green, .4f);
                if (m.z == 1) AxisRay(point, c, Color.blue, .4f);
            }
        }
    }

    public static void AxisRay(Vector3 point, Vector3 axis, Color color, float a = .2f)
    {
        color.a = a;
        Handles.color = color;
        Handles.DrawLine(point - axis * 1e5f, point + axis * 1e5f);
    }
}