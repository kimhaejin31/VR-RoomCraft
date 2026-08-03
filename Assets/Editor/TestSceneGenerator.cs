using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class TestSceneGenerator
{
    [MenuItem("VR-RoomCraft/Generate Test Scene")]
    public static void GenerateTestScene()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Create Directional Light
        GameObject lightObject = new GameObject("Directional Light");
        Light lightComponent = lightObject.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.color = Color.white;
        lightComponent.intensity = 1.0f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        lightObject.transform.position = new Vector3(0f, 3f, 0f);

        // 2. Create Plane
        GameObject planeObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeObject.name = "Plane";
        planeObject.transform.position = Vector3.zero;

        // 3. Create Cube
        GameObject cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeObject.name = "Cube";
        cubeObject.transform.position = new Vector3(0f, 0.5f, 0f);

        // Save scene
        string scenePath = "Assets/Scenes/TestScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[VR-RoomCraft] TestScene saved successfully to: {scenePath}");
    }
}
