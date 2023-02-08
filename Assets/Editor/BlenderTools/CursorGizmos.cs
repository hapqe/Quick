using UnityEngine;
using UnityEditor;
using System.Linq;
using static UnityEngine.Vector3;

public static class CursorGizmos
{
    public static void Draw()
    {
        var pos = Cursor.position;
        var cam = UnityEditor.SceneView.lastActiveSceneView.camera;
        var scale = 1f / cam.pixelWidth * 40f;
        var camPlane = new Plane(cam.transform.forward, cam.transform.position);
        scale *= camPlane.GetDistanceToPoint(pos);

        Handles.color = Color.black;

        new Vector3[] { right, up, left, down, forward, back }
        .ToList()
        .ForEach(v => DrawLine(pos, scale, v));

        DrawCircleAlignedToView(16, scale, pos);
    }

    private static void DrawLine(Vector3 pos, float scale, Vector3 v)
    {
        var line = ((v * .5f) * scale, (v * 1.5f) * scale);
        Handles.DrawLine(pos + line.Item1, pos + line.Item2);
    }

    static void DrawCircleAlignedToView(int sub, float radius, Vector3 origin, Color[] colors = null)
    {
        colors = colors ?? new Color[] { Color.white, Color.red };
        
        var cam = UnityEditor.SceneView.lastActiveSceneView.camera;
        var up = cam.transform.up;
        var right = cam.transform.right;
        var center = origin;

        var last = center + right * radius;
        for (int i = 0; i < sub; i++)
        {
            var angle = (i + 1) * 2 * Mathf.PI / sub;
            var pos = center + Mathf.Cos(angle) * right * radius + Mathf.Sin(angle) * up * radius;
            Handles.color = colors[i % colors.Length];
            Handles.DrawLine(last, pos);
            last = pos;
        }
    }
}