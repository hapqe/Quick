using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public interface IStateTool {
    Predicate<Event> trigger { get; }
    void Start();
    void Update(SceneView sceneView);
    void AfterUpdate();
    void Perform();
    void Cancel();
    void Numerical(float input);
}

[InitializeOnLoad]
public static class EditorState
{
    public static IStateTool active { get; private set; } = null;
    public static List<IStateTool> tools = new List<IStateTool>();
    static EditorState()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private static void OnScene(SceneView sceneView)
    {
        Event e = Event.current;

        if(active == null) {
            // keydown
            if(e.type == EventType.KeyDown) {
                foreach(var tool in tools) {
                    if(tool.trigger(e)) {
                        active = tool;
                        active.Start();
                        break;
                    }
                }
            }
        }
        else {
            // lmb or enter
            if(e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyDown && e.keyCode == KeyCode.Return) {
                e.Use();
                
                active.Perform();
                active = null;
                return;
            }
            // rmb or esc
            if(e.type == EventType.MouseDown && e.button == 1 || e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) {
                e.Use();
                
                active.Cancel();
                active = null;
                return;
            }
            // update
            active.Update(sceneView);
            active.AfterUpdate();
        }
    }
}