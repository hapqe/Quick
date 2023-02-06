using System;
using UnityEditor;
using UnityEngine;

public class EditorInputDialog : EditorWindow
{
    string description, inputText;
    string okButton, cancelButton;
    bool initializedPosition = false;
    Action onOKButton;

    bool shouldClose = false;

    #region OnGUI()
    void OnGUI()
    {
        // Check if Esc/Return have been pressed
        var e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                // Escape pressed
                case KeyCode.Escape:
                    shouldClose = true;
                    break;

                // Enter pressed
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    onOKButton?.Invoke();
                    shouldClose = true;
                    break;
            }
        }

        if (shouldClose)
        {  // Close this dialog
            Close();
            //return;
        }

        // Draw our control
        var rect = EditorGUILayout.BeginVertical();

        GUI.SetNextControlName("inText");
        inputText = EditorGUILayout.TextField("", inputText);
        GUI.FocusControl("inText");   // Focus text field
        EditorGUILayout.Space(12);

        EditorGUILayout.EndVertical();

        // Force change size of the window
        if (rect.width != 0 && minSize != rect.size)
        {
            minSize = maxSize = rect.size;
        }

        // Set dialog position next to mouse position
        if (!initializedPosition)
        {
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            position = new Rect(mousePos.x + 32, mousePos.y, position.width, position.height);
            initializedPosition = true;
        }
    }
    #endregion OnGUI()

    #region Show()
    /// <summary>
    /// Returns text player entered, or null if player cancelled the dialog.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="inputText"></param>
    /// <param name="okButton"></param>
    /// <param name="cancelButton"></param>
    /// <returns></returns>
    public static string Show(string title)
    {
        string ret = null;
        var window = CreateInstance<EditorInputDialog>();
        window.titleContent = new GUIContent(title);
        window.onOKButton += () => ret = window.inputText;
        window.ShowModal();

        return ret;
    }
    #endregion Show()
}
