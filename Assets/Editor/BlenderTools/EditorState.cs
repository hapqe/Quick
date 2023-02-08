using UnityEngine;
using UnityEditor;
using System;


public static class EditorState {
    public static bool active { get; internal set; }
}
abstract class StateTool<T> where T : StateTool<T> {
    internal static StateTool<T> active { get; set; }
    internal static void MakeActive() {
        if(active == null){
            var a = (T)Activator.CreateInstance(typeof(T));
            if(!a.Requirements()) return;
            
            active = a;
            EditorState.active = true;
            SceneView.duringSceneGui += Activate;
        }
    }
    internal static void Reset() {
        if(active != null) {
            SceneView.duringSceneGui -= Perform;
            EditorState.active = false;
            active = null;
        }
    }
    internal abstract Action again { get; }
    internal virtual bool Requirements() => true;
    internal abstract void Start();
    internal abstract void Update(SceneView sceneView);
    internal abstract void AfterUpdate();
    internal abstract void Perform();
    internal abstract void Cancel();
    internal abstract void Numerical(float input);

    public static void Activate(SceneView sceneView) {
        StateTool<T>.active.Start();
        SceneView.duringSceneGui += Perform;
        SceneView.duringSceneGui -= Activate;
    }
    public static void Perform(SceneView sceneView)
    {
        var e = Event.current;
        var active = StateTool<T>.active;

        if(active != null) {
            // trigger twice
            
            // setting control, so events like lmb are not forwarded to the scene
            var id = GUIUtility.GetControlID(active.GetHashCode(), FocusType.Passive);
            if(e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(id);
            // lmb or enter
            if(e.type == EventType.MouseUp && e.button == 0 || e.type == EventType.KeyDown && e.keyCode == KeyCode.Return) {
                e.Use();
                
                active.Perform();
                StateTool<T>.Reset();
                active = null;
                return;
            }
            // rmb or esc
            if(e.type == EventType.MouseDown && e.button == 1 || e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) {
                e.Use();
                
                active.Cancel();
                StateTool<T>.Reset();
                active = null;
                return;
            }

            // update
            active.Update(sceneView);
            active.AfterUpdate();
        }
    }
}