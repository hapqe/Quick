using UnityEngine;

public class CursorGizmos : MonoBehaviour, IGizmosDrawer
{
    public float scale;
    public bool ortho = false;
    public void OnDrawGizmos()
    {
        var cam = UnityEditor.SceneView.lastActiveSceneView.camera;
        
        var cameraPlane = new Plane(cam.transform.forward, cam.transform.position);
        var d = cameraPlane.GetDistanceToPoint(transform.position);

        var pos = transform.position;

        if(cam.orthographic){
            var dir = cam.transform.forward;
            d = 1.5f;

            var screenPos = cam.WorldToScreenPoint(pos);
            pos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, d));

            scale = cam.orthographicSize;
        }
        
        Gizmos.color = Color.red;
        #if UNITY_EDITOR
        var a = .04f * scale;
        var b = .06f * scale;
        var c = .02f * scale;
        
        DrawCircleAlignedToView(16, d * a, pos);
        Gizmos.color = Color.black;
        // right
        Gizmos.DrawLine(pos + transform.forward * d * c, pos + transform.forward * d * b);
        // left
        Gizmos.DrawLine(pos - transform.forward * d * c, pos - transform.forward * d * b);
        // up
        Gizmos.DrawLine(pos + transform.up * d * c, pos + transform.up * d * b);
        // down
        Gizmos.DrawLine(pos - transform.up * d * c, pos - transform.up * d * b);
        // forward
        Gizmos.DrawLine(pos + transform.right * d * c, pos + transform.right * d * b);
        // back
        Gizmos.DrawLine(pos - transform.right * d * c, pos - transform.right * d * b);
        #endif
    }

    void DrawCircleAlignedToView(int sub, float radius, Vector3 origin, Color[] colors = null)
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
            Gizmos.color = colors[i % colors.Length];
            Gizmos.DrawLine(last, pos);
            last = pos;
        }
    }
}