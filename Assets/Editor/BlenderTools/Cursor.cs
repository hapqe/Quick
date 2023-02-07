using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class Cursor
{
    public static Vector3 position;
    
    static Cursor()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private static void OnScene(SceneView sceneview)
    {
        Event e = Event.current;
        if(!e.shift || EditorState.active != null) return;
        
        EditorHelpers.GetDrawer<CursorGizmos>().transform.position = position;
        EditorHelpers.GetDrawer<CursorGizmos>().scale = .7f / sceneview.position.width * 1000f;
        

        // shift-lmb
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 1)
        {
            e.Use();

            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            var renderers = GameObject.FindObjectsOfType<MeshFilter>();
            var hits = new List<RaycastHit>();
             
            // raycast all renderers, and get the closest hit
            foreach (var renderer in renderers)
            {
                var collider = renderer.gameObject.AddComponent<MeshCollider>();
                
                if (collider.Raycast(ray, out var hit, float.PositiveInfinity))
                {
                    hits.Add(hit);
                }

                Object.DestroyImmediate(collider);
            }

            if (hits.Count > 0)
            {
                hits.Sort((a, b) => a.distance.CompareTo(b.distance));
                position = hits[0].point;
            }
            // if no hits:
            else {
                // use origin if in 2D mode
                if(sceneview.in2DMode)
                    position = new Vector3(ray.origin.x, ray.origin.y, 0);
                // else use xy plane
                else {
                    var plane = new Plane(Vector3.up, Vector3.zero);
                    if(plane.Raycast(ray, out var distance))
                        position = ray.GetPoint(distance);
                }
            }
        };

        // shift-c
        if(e.keyCode == KeyCode.C)
            position = Vector3.zero;
    }
}