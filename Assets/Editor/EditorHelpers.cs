using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorHelpers
{
    public static T GetDrawer<T>() where T : MonoBehaviour, IGizmosDrawer {
        var drawer = Object.FindObjectOfType<T>();
        if(drawer == null) {
            drawer = new GameObject(nameof(T)).AddComponent<T>();
            drawer.transform.hideFlags = HideFlags.HideInHierarchy;
        }
        return drawer;
    }
}
