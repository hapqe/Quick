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
[InitializeOnLoad]
class Pivot : EditorToolbarDropdown
{
    static Pivot()
    {
        Tools.pivotModeChanged += () =>
        {
            switch (Tools.pivotMode)
            {
                case UnityEditor.PivotMode.Pivot:
                    mode = PivotMode.Individual;
                    break;
                case UnityEditor.PivotMode.Center:
                    mode = PivotMode.Median;
                    break;
            }
            onChange?.Invoke();
        };
    }
    public const string id = "PivotToolBar/Dropdown";

    public static PivotMode mode = PivotMode.Median;

    public static void CycleMode()
    {
        mode = (PivotMode)(((int)mode + 1) % Enum.GetNames(typeof(PivotMode)).Length);
        onChange?.Invoke();
    }

    static Action onChange;

    public Pivot()
    {
        text = "Pivot";
        clicked += ShowDropdown;

        onChange += PerformCycle;

        PerformCycle();
    }

    private void PerformCycle()
    {
        switch (mode)
        {
            case PivotMode.Individual:
                icon = EditorGUIUtility.IconContent("d_ToolHandleLocal").image as Texture2D;
                Tools.pivotMode = UnityEditor.PivotMode.Pivot;
                break;
            case PivotMode.Median:
                icon = EditorGUIUtility.IconContent("d_ToolHandleGlobal").image as Texture2D;
                Tools.pivotMode = UnityEditor.PivotMode.Center;
                break;
            case PivotMode.Cursor:
                icon = EditorGUIUtility.IconContent("d_Animation.FilterBySelection").image as Texture2D;
                break;
        }
    }

    void ShowDropdown()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Individual Origins"), mode == PivotMode.Individual, () => { mode = PivotMode.Individual; onChange?.Invoke(); });
        menu.AddItem(new GUIContent("Median Point"), mode == PivotMode.Median, () => { mode = PivotMode.Median; onChange?.Invoke(); });
        menu.AddItem(new GUIContent("3D-Cursor"), mode == PivotMode.Cursor, () => { mode = PivotMode.Cursor; onChange?.Invoke(); });
        menu.ShowAsContext();
    }
}

[Overlay(typeof(SceneView), "PivotToolBar")]
[Icon("Assets/Editor/Icons/Cursor.png")]
public class EditorToolbarExample : ToolbarOverlay
{
    EditorToolbarExample() : base(Pivot.id)
    { }
}