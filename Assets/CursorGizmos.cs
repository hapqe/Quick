using UnityEngine;

public class CursorGizmos : MonoBehaviour, IGizmosDrawer
{
    public void OnDrawGizmos()
    {
        var cam = UnityEditor.SceneView.lastActiveSceneView.camera;
        
        var cameraPlane = new Plane(cam.transform.forward, cam.transform.position);
        var d = cameraPlane.GetDistanceToPoint(transform.position);
        
        Gizmos.color = Color.red;
        #if UNITY_EDITOR
        DrawCircleAlignedToView(16, d * .04f, transform.position);
        Gizmos.color = Color.black;
        // right
        Gizmos.DrawLine(transform.position + transform.forward * d * .02f, transform.position + transform.forward * d * .06f);
        // left
        Gizmos.DrawLine(transform.position - transform.forward * d * .02f, transform.position - transform.forward * d * .06f);
        // up
        Gizmos.DrawLine(transform.position + transform.up * d * .02f, transform.position + transform.up * d * .06f);
        // down
        Gizmos.DrawLine(transform.position - transform.up * d * .02f, transform.position - transform.up * d * .06f);
        // forward
        Gizmos.DrawLine(transform.position + transform.right * d * .02f, transform.position + transform.right * d * .06f);
        // back
        Gizmos.DrawLine(transform.position - transform.right * d * .02f, transform.position - transform.right * d * .06f);
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