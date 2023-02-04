using UnityEngine;

public class CursorGizmos : MonoBehaviour, IGizmosDrawer
{
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}