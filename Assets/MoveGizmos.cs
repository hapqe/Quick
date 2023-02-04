using UnityEngine;

public class MoveGizmos : MonoBehaviour, IGizmosDrawer
{
    public Vector3 mean;
    public bool showAll = false;
    public Vector3 delta;
    public Vector3 mask = Vector3.one;
    public void OnDrawGizmos()
    {
        if(showAll) {
            AxisRay(Vector3.right, Color.red);
            AxisRay(Vector3.up, Color.green);
            AxisRay(Vector3.forward, Color.blue);
            var c = Color.white;
            c.a = .2f;
            Gizmos.color = c;
            Gizmos.DrawLine(mean, mean + delta);
        }

        if(mask != Vector3.one) {
            if(mask.x == 1) AxisRay(Vector3.right, Color.red, .4f);
            if(mask.y == 1) AxisRay(Vector3.up, Color.green, .4f);
            if(mask.z == 1) AxisRay(Vector3.forward, Color.blue, .4f);
        }
    }

    public void AxisRay(Vector3 axis, Color color, float a = .2f)
    {
        color.a = a;
        Gizmos.color = color;
        Gizmos.DrawRay(mean - axis * 1e5f, axis * 1e6f);
    }
}