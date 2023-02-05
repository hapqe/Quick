using System.Collections.Generic;
using UnityEngine;

public class TransformGizmos : MonoBehaviour, IGizmosDrawer
{
    public bool show;
    
    public Vector3 point;
    public bool showAll = false;
    public Vector3 delta;
    public Vector3? mask;

    public Vector3[] directions;
    public Vector3[] points;
    
    public void OnDrawGizmos()
    {
        if(!show) return;
        
        var color = Color.white;
        color.a = .2f;
        Gizmos.color = color;
        Gizmos.DrawLine(point, point + delta);

        for (int i = 0; i < directions.Length; i += 3)
        {
            var a = directions[i];
            var b = directions[i + 1];
            var c = directions[i + 2];
            
            var point = points[i / 3];
            
            if(showAll) {
                AxisRay(point, a, Color.red, .2f);
                AxisRay(point, b, Color.green, .2f);
                AxisRay(point, c, Color.blue, .2f);
            }

            if(mask != null) {
                var m = (Vector3)mask;
                if(m.x == 1) AxisRay(point, a, Color.red, .4f);
                if(m.y == 1) AxisRay(point, b, Color.green, .4f);
                if(m.z == 1) AxisRay(point, c, Color.blue, .4f);
            }
        }

    }

    public void AxisRay(Vector3 point, Vector3 axis, Color color, float a = .2f)
    {
        color.a = a;
        Gizmos.color = color;
        Gizmos.DrawRay(point - axis * 1e5f, axis * 1e6f);
    }
}