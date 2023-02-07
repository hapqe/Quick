using System;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;

public enum PivotMode
{
    Median,
    Individual,
    Cursor
}

[EditorToolbarElement(id, typeof(SceneView))]
class PivotDropdown : EditorToolbarDropdown
{
    public const string id = "PivotToolBar/Dropdown";

    public static PivotMode pivotMode = PivotMode.Median;

    public static void CycleMode() {
        pivotMode = (PivotMode)(((int)pivotMode + 1) % Enum.GetNames(typeof(PivotMode)).Length);
        onChange?.Invoke();
    }

    static Action onChange;

    public PivotDropdown()
    {
        text = "Pivot";
        clicked += ShowDropdown;

        onChange += ChangeIcon;

        ChangeIcon();
    }

    private void ChangeIcon()
    {
        switch (pivotMode)
        {
            case PivotMode.Individual:
                icon = EditorGUIUtility.IconContent("d_ToolHandleLocal").image as Texture2D;
                break;
            case PivotMode.Median:
                icon = EditorGUIUtility.IconContent("d_ToolHandleGlobal").image as Texture2D;
                break;
            case PivotMode.Cursor:
                icon = EditorGUIUtility.IconContent("d_Animation.FilterBySelection").image as Texture2D;
                break;
        }
    }

    void ShowDropdown()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Individual"), pivotMode == PivotMode.Individual, () => { pivotMode = PivotMode.Individual; onChange?.Invoke(); });
        menu.AddItem(new GUIContent("Median Point"), pivotMode == PivotMode.Median, () => { pivotMode = PivotMode.Median; onChange?.Invoke(); });
        menu.AddItem(new GUIContent("Cursor"), pivotMode == PivotMode.Cursor, () => { pivotMode = PivotMode.Cursor; onChange?.Invoke(); });
        menu.ShowAsContext();
    }
}

[Overlay(typeof(SceneView), "PivotToolBar")]
[Icon("Assets/Editor/Icons/Cursor.png")]
public class EditorToolbarExample : ToolbarOverlay
{
    EditorToolbarExample() : base(
PivotDropdown.id
)
    { }

}