using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

public static class TypingTool
{
    public static void Typing(System.Action perform, System.Func<Event, bool> trigger, ref string name, ref bool typing){
        var e = Event.current;
        var k = e.keyCode;
        var d = e.type == EventType.KeyDown;
        var m = e.type == EventType.MouseDown;
        
        if(trigger(e)) {
            Event.current.Use();
            
            var selection = Selection.gameObjects;
            
            if(selection.Length == 0) return;

            typing = true;

            return;
        }

        if(!typing) return;

        if(d && EditorHelpers.IsLetter(k)) {
            Event.current.Use();
            var c = Event.current.keyCode.ToString();
            if(!e.shift) c = c.ToLower();
            name += c;
        }
        if(d && EditorHelpers.IsNumber(k)) {
            Event.current.Use();
            name += Event.current.keyCode.ToString().Substring(5);
        }
        if(d && k == KeyCode.Backspace) {
            Event.current.Use();
            if(name.Length > 0) name = name.Substring(0, name.Length - 1);
        }

        // esc or rmb
        if(d && k == KeyCode.Escape || m && e.button == 1) {
            Event.current.Use();
            typing = false;
            name = "";
        }

        // enter or lmb
        if(d && k == KeyCode.Return || m && e.button == 0)
        {
            Event.current.Use();
            typing = false;
            perform?.Invoke();
            Collapse();
            name = "";
        }

        if (!(e.type == EventType.Layout || e.type == EventType.Repaint))
        e.Use();
    }
}