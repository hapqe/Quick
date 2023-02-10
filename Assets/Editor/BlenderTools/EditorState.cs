using UnityEngine;
using UnityEditor;
using System;
using static EditorHelpers;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;

public static class EditorState {
    public static bool active { get; internal set; }
}
abstract class StateTool<T> where T : StateTool<T> {
    string input = "";
    protected bool inputValid { get => float.TryParse(input, out _); }

    static float preciseFactor = 0.1f;
    protected float precise => Event.current.shift ? preciseFactor : 1;
    protected Vector2 absoluteDelta { get; private set; }
    protected Vector2 delta { get; private set; }
    protected Vector2 currentDelta { get; private set; }
    protected Vector2 start { get; private set; }
    protected Vector2 mouse { get => start + delta; }
    internal static StateTool<T> active { get; set; }
    IEnumerable<KeyCombination> shortcuts;
    internal static void MakeActive(string menuName) {
        // remove from last space
        menuName = menuName.Substring(0, menuName.LastIndexOf(' '));

        var shortcuts = ShortcutManager.instance.GetShortcutBinding("Main Menu/" + menuName).keyCombinationSequence;
        
        if(active == null){
            var a = (T)Activator.CreateInstance(typeof(T));
            if(!a.Requirements()) return;

            a.shortcuts = shortcuts;
            active = a;
            EditorState.active = true;
            SceneView.duringSceneGui += Activate;

            // focus on scene view
            var sceneView = SceneView.lastActiveSceneView;
            if(sceneView != null) {
                sceneView.Focus();
            }
        }
    }
    internal static void Reset() {
        if(active != null) {
            SceneView.duringSceneGui -= Perform;
            EditorState.active = false;
            active = null;
        }
    }
    internal virtual bool Requirements() => true;
    internal virtual void Start() {
        start = Event.current.mousePosition;
        delta = Vector2.zero;
        input = "";
    }
    internal virtual void Update(SceneView sceneView) {
        var e = Event.current;

        if(TestShortcuts(e, shortcuts)) {
            Again();
            return;
        }

        Continuous(sceneView, e);
        
        var screen = LogicalRect(Camera.current.pixelRect);
        if (Math.Abs(e.delta.x) < screen.width - 50 && Math.Abs(e.delta.y) < screen.height - 50) {
            // "world" space
            var d = e.delta;
            delta += d * precise;
            currentDelta = d * precise;

            // screen space
            d.y *= -1;
            d *= EditorGUIUtility.pixelsPerPoint;
            absoluteDelta += d;
        }
    }
    internal virtual void AfterUpdate() {
        AppendEvent(Event.current, ref input);

        if(inputValid) {
            Numerical(float.Parse(input));
        }
    }

    internal abstract void Perform();
    internal abstract void Cancel();
    internal abstract void Numerical(float input);
    internal virtual void Again() { }

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

    private static void Continuous(SceneView sceneView, Event e)
    {
        var rect = PhysicalRect(sceneView.position);
        var screen = Camera.current.pixelRect;

        var m = PhysicalPoint(e.mousePosition);

        var navHeight = (int)(rect.height - screen.height);
        var margin = 10;
        var innerMargin = 20;

        var x = (int)(m.x + rect.min.x);
        if (m.y < margin)
        {
            Rust.set_cursor_pos(x, (int)rect.max.y);
        }
        if (m.y > screen.height - margin)
        {
            Rust.set_cursor_pos(x, (int)rect.min.y + navHeight + innerMargin * 2);
        }

        var y = (int)(m.y + rect.min.y + navHeight);
        if (m.x < margin)
        {
            Rust.set_cursor_pos((int)rect.max.x - innerMargin, y);
        }
        if (m.x > screen.width - margin)
        {
            Rust.set_cursor_pos((int)rect.min.x + innerMargin, y);
        }
    }

    protected virtual float snap { get => 1; }
    float snapFactor { get => Event.current.control ? snap : 0; }
    protected float Snap(float v) => Snapping.Snap(v, snapFactor);
    protected Vector2 Snap(Vector2 v) => Snapping.Snap(v, snapFactor * Vector2.one);
    protected Vector3 Snap(Vector3 v) => Snapping.Snap(v, snapFactor * Vector3.one);
}