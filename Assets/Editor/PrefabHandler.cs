using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static EditorHelpers;

[InitializeOnLoad]
public static class PrefabHandler
{
    static PrefabHandler()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private static void OnScene(SceneView sceneView){
        Event e = Event.current;
        var k = e.keyCode;
        var d = e.type == EventType.KeyDown;
        var m = e.type == EventType.MouseDown;

        // shift-p
        if(e.shift && d && k == KeyCode.P) {
            e.Use();

            CreatePrefab();
        }

        // shift-a
        if(e.shift && d && k == KeyCode.A) {
            e.Use();

            ApplyOverrides();
        }

        // shift-u
        if(e.shift && d && k == KeyCode.U) {
            e.Use();

            UnpackPrefab();
        }
    }

    static void CreatePrefab()
    {
        // https://docs.unity3d.com/ScriptReference/PrefabUtility.html

        var objectArray = Selection.gameObjects;

        foreach (GameObject gameObject in objectArray)
        {
            if (!Directory.Exists("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            string localPath = "Assets/Prefabs/" + gameObject.name + ".prefab";

            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, localPath, InteractionMode.UserAction, out _);
        }
    }
    
    static void ApplyOverrides(){
        var objectArray = Selection.gameObjects;

        foreach (GameObject gameObject in objectArray)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if(prefab == null) continue;
            
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.UserAction);
        }
    }

    static void UnpackPrefab(){
        var objectArray = Selection.gameObjects;

        foreach (GameObject gameObject in objectArray)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if(prefab == null) continue;
            
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }
    }
}
