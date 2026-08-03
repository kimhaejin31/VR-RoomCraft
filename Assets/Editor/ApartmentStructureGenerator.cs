using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ApartmentStructureGenerator
{
    [MenuItem("VR-RoomCraft/Build Apartment Scene")]
    public static void GenerateApartmentScene()
    {
        // 1. Create a new scene setup
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Main Lighting & Camera Setup optimized for VR
        SetupEnvironmentLighting();

        // 3. Create Root GameObject: Apartment
        GameObject apartmentRoot = new GameObject("Apartment");
        apartmentRoot.transform.position = Vector3.zero;
        apartmentRoot.transform.rotation = Quaternion.identity;
        apartmentRoot.transform.localScale = Vector3.one;

        // 4. Create Containers according to exact specified hierarchy
        GameObject wallsContainer = CreateChildContainer(apartmentRoot, "Walls");
        GameObject floorObj = CreateFloor(apartmentRoot);
        GameObject ceilingObj = CreateCeiling(apartmentRoot);
        GameObject doorsContainer = CreateChildContainer(apartmentRoot, "Doors");
        GameObject windowsContainer = CreateChildContainer(apartmentRoot, "Windows");
        GameObject kitchenContainer = CreateChildContainer(apartmentRoot, "Kitchen");
        GameObject bedroomContainer = CreateChildContainer(apartmentRoot, "Bedroom");
        GameObject bathroomContainer = CreateChildContainer(apartmentRoot, "Bathroom");
        GameObject livingRoomContainer = CreateChildContainer(apartmentRoot, "LivingRoom");
        GameObject closetContainer = CreateChildContainer(apartmentRoot, "Closet");

        // 5. Build Apartment Structure (All GameObjects are real Cube Primitives)
        BuildWalls(wallsContainer);
        BuildDoors(doorsContainer);
        BuildWindows(windowsContainer);
        BuildKitchen(kitchenContainer);

        // 6. Setup Room Zone Markers
        SetupZoneAnchor(bedroomContainer, "Bedroom_Zone", new Vector3(-1.5f, 0f, 2.4f));
        SetupZoneAnchor(bathroomContainer, "Bathroom_Zone", new Vector3(2.0f, 0f, 2.4f));
        SetupZoneAnchor(livingRoomContainer, "LivingRoom_Zone", new Vector3(0.5f, 0f, -1.8f));
        SetupZoneAnchor(closetContainer, "Closet_Zone", new Vector3(2.0f, 0f, 0.65f));

        // 7. Mark static for Unity 6 VR optimization
        SetStaticRecursively(apartmentRoot);

        // 8. Save Scene to Assets/Scenes/ApartmentScene.unity
        string scenePath = "Assets/Scenes/ApartmentScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[VR-RoomCraft] Apartment Structure Scene generated and saved to: {scenePath}");
    }

    private static void SetStaticRecursively(GameObject obj)
    {
        obj.isStatic = true;
        foreach (Transform child in obj.transform)
        {
            SetStaticRecursively(child.gameObject);
        }
    }

    private static void SetupEnvironmentLighting()
    {
        // Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light lightComponent = lightObj.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.color = new Color(1.0f, 0.96f, 0.90f);
        lightComponent.intensity = 1.2f;
        lightComponent.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        lightObj.transform.position = new Vector3(0f, 3.5f, 0f);

        // VR Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cameraComponent = cameraObj.AddComponent<Camera>();
        cameraComponent.tag = "MainCamera";
        cameraObj.AddComponent<AudioListener>();
        cameraObj.transform.position = new Vector3(0f, 1.6f, -1.5f);
        cameraObj.transform.rotation = Quaternion.identity;
    }

    private static GameObject CreateChildContainer(GameObject parent, string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        return container;
    }

    // Floor: 6m x 7m studio footprint (Cube Primitive)
    private static GameObject CreateFloor(GameObject parent)
    {
        GameObject floor = CreatePrimitiveBox("Floor", new Vector3(0f, -0.05f, 0f), new Vector3(6.0f, 0.1f, 7.0f));
        floor.transform.SetParent(parent.transform, false);
        return floor;
    }

    // Ceiling: 6m x 7m studio footprint at 2.8m height (Cube Primitive)
    private static GameObject CreateCeiling(GameObject parent)
    {
        GameObject ceiling = CreatePrimitiveBox("Ceiling", new Vector3(0f, 2.85f, 0f), new Vector3(6.0f, 0.1f, 7.0f));
        ceiling.transform.SetParent(parent.transform, false);
        return ceiling;
    }

    private static void BuildWalls(GameObject parent)
    {
        // Wall North (Back wall, Z = +3.5m)
        CreateWallSegment(parent, "Wall_North_Left", new Vector3(-2.0f, 1.4f, 3.575f), new Vector3(2.0f, 2.8f, 0.15f));
        CreateWallSegment(parent, "Wall_North_Right", new Vector3(2.0f, 1.4f, 3.575f), new Vector3(2.0f, 2.8f, 0.15f));
        CreateWallSegment(parent, "Wall_North_WindowTop", new Vector3(0f, 2.35f, 3.575f), new Vector3(2.0f, 0.9f, 0.15f));
        CreateWallSegment(parent, "Wall_North_WindowBottom", new Vector3(0f, 0.45f, 3.575f), new Vector3(2.0f, 0.9f, 0.15f));

        // Wall South (Front entrance wall, Z = -3.5m)
        CreateWallSegment(parent, "Wall_South_Left", new Vector3(-2.65f, 1.4f, -3.575f), new Vector3(0.7f, 2.8f, 0.15f));
        CreateWallSegment(parent, "Wall_South_DoorTop", new Vector3(-1.8f, 2.45f, -3.575f), new Vector3(1.0f, 0.7f, 0.15f));
        CreateWallSegment(parent, "Wall_South_Mid", new Vector3(0.1f, 1.4f, -3.575f), new Vector3(2.8f, 2.8f, 0.15f));
        CreateWallSegment(parent, "Wall_South_WindowTop", new Vector3(2.0f, 2.35f, -3.575f), new Vector3(1.0f, 0.9f, 0.15f));
        CreateWallSegment(parent, "Wall_South_WindowBottom", new Vector3(2.0f, 0.45f, -3.575f), new Vector3(1.0f, 0.9f, 0.15f));
        CreateWallSegment(parent, "Wall_South_Right", new Vector3(2.75f, 1.4f, -3.575f), new Vector3(0.5f, 2.8f, 0.15f));

        // Wall East (Right exterior wall, X = +3.0m)
        CreateWallSegment(parent, "Wall_East", new Vector3(3.075f, 1.4f, 0f), new Vector3(0.15f, 2.8f, 7.0f));

        // Wall West (Left exterior wall, X = -3.0m)
        CreateWallSegment(parent, "Wall_West", new Vector3(-3.075f, 1.4f, 0f), new Vector3(0.15f, 2.8f, 7.0f));

        // Interior Partition Walls
        CreateWallSegment(parent, "Wall_Bathroom_West", new Vector3(0.95f, 1.4f, 2.4f), new Vector3(0.1f, 2.8f, 2.2f));
        CreateWallSegment(parent, "Wall_Bathroom_South_Left", new Vector3(1.35f, 1.4f, 1.35f), new Vector3(0.7f, 2.8f, 0.1f));
        CreateWallSegment(parent, "Wall_Bathroom_South_DoorTop", new Vector3(2.0f, 2.45f, 1.35f), new Vector3(0.6f, 0.7f, 0.1f));
        CreateWallSegment(parent, "Wall_Bathroom_South_Right", new Vector3(2.65f, 1.4f, 1.35f), new Vector3(0.7f, 2.8f, 0.1f));

        CreateWallSegment(parent, "Wall_Closet_West", new Vector3(0.95f, 1.4f, 0.65f), new Vector3(0.1f, 2.8f, 1.3f));
        CreateWallSegment(parent, "Wall_Closet_South_Left", new Vector3(1.25f, 1.4f, 0.05f), new Vector3(0.5f, 2.8f, 0.1f));
        CreateWallSegment(parent, "Wall_Closet_South_DoorTop", new Vector3(2.0f, 2.45f, 0.05f), new Vector3(1.0f, 0.7f, 0.1f));
        CreateWallSegment(parent, "Wall_Closet_South_Right", new Vector3(2.75f, 1.4f, 0.05f), new Vector3(0.5f, 2.8f, 0.1f));

        CreateWallSegment(parent, "Wall_Bedroom_Partition", new Vector3(-2.0f, 1.4f, 1.35f), new Vector3(2.0f, 2.8f, 0.1f));
    }

    private static void CreateWallSegment(GameObject parent, string name, Vector3 localPos, Vector3 localScale)
    {
        GameObject wall = CreatePrimitiveBox(name, localPos, localScale);
        wall.transform.SetParent(parent.transform, false);
    }

    private static void BuildDoors(GameObject parent)
    {
        // 1. Entrance Door
        GameObject entranceDoorGroup = new GameObject("Entrance_Door");
        entranceDoorGroup.transform.SetParent(parent.transform, false);
        entranceDoorGroup.transform.localPosition = new Vector3(-1.8f, 0f, -3.575f);

        GameObject entranceFrame = CreatePrimitiveBox("Entrance_DoorFrame", new Vector3(0f, 1.05f, 0f), new Vector3(1.0f, 2.1f, 0.18f));
        entranceFrame.transform.SetParent(entranceDoorGroup.transform, false);

        GameObject entrancePanel = CreatePrimitiveBox("Entrance_DoorPanel", new Vector3(0f, 1.025f, 0f), new Vector3(0.9f, 2.05f, 0.05f));
        entrancePanel.transform.SetParent(entranceDoorGroup.transform, false);

        // 2. Bathroom Door
        GameObject bathroomDoorGroup = new GameObject("Bathroom_Door");
        bathroomDoorGroup.transform.SetParent(parent.transform, false);
        bathroomDoorGroup.transform.localPosition = new Vector3(2.0f, 0f, 1.35f);

        GameObject bathroomFrame = CreatePrimitiveBox("Bathroom_DoorFrame", new Vector3(0f, 1.05f, 0f), new Vector3(0.6f, 2.1f, 0.12f));
        bathroomFrame.transform.SetParent(bathroomDoorGroup.transform, false);

        GameObject bathroomPanel = CreatePrimitiveBox("Bathroom_DoorPanel", new Vector3(0f, 1.025f, 0f), new Vector3(0.54f, 2.05f, 0.04f));
        bathroomPanel.transform.SetParent(bathroomDoorGroup.transform, false);
    }

    private static void BuildWindows(GameObject parent)
    {
        // 1. Bedroom Window
        GameObject bedroomWindowGroup = new GameObject("Bedroom_Window");
        bedroomWindowGroup.transform.SetParent(parent.transform, false);
        bedroomWindowGroup.transform.localPosition = new Vector3(0f, 1.4f, 3.575f);

        GameObject bedroomFrame = CreatePrimitiveBox("Bedroom_WindowFrame", Vector3.zero, new Vector3(2.0f, 1.0f, 0.18f));
        bedroomFrame.transform.SetParent(bedroomWindowGroup.transform, false);

        GameObject bedroomGlass = CreatePrimitiveBox("Bedroom_WindowGlass", Vector3.zero, new Vector3(1.85f, 0.85f, 0.02f));
        bedroomGlass.transform.SetParent(bedroomWindowGroup.transform, false);

        // 2. LivingRoom Window
        GameObject livingWindowGroup = new GameObject("LivingRoom_Window");
        livingWindowGroup.transform.SetParent(parent.transform, false);
        livingWindowGroup.transform.localPosition = new Vector3(2.0f, 1.4f, -3.575f);

        GameObject livingFrame = CreatePrimitiveBox("LivingRoom_WindowFrame", Vector3.zero, new Vector3(1.0f, 1.0f, 0.18f));
        livingFrame.transform.SetParent(livingWindowGroup.transform, false);

        GameObject livingGlass = CreatePrimitiveBox("LivingRoom_WindowGlass", Vector3.zero, new Vector3(0.88f, 0.88f, 0.02f));
        livingGlass.transform.SetParent(livingWindowGroup.transform, false);
    }

    private static void BuildKitchen(GameObject parent)
    {
        GameObject baseCabinets = CreatePrimitiveBox("Kitchen_BaseCabinets", new Vector3(-2.0f, 0.45f, -2.2f), new Vector3(1.8f, 0.9f, 0.6f));
        baseCabinets.transform.SetParent(parent.transform, false);

        GameObject countertop = CreatePrimitiveBox("Kitchen_Countertop", new Vector3(-2.0f, 0.925f, -2.2f), new Vector3(1.85f, 0.05f, 0.65f));
        countertop.transform.SetParent(parent.transform, false);

        GameObject sink = CreatePrimitiveBox("Kitchen_Sink", new Vector3(-2.3f, 0.96f, -2.2f), new Vector3(0.5f, 0.04f, 0.45f));
        sink.transform.SetParent(parent.transform, false);

        GameObject upperCabinets = CreatePrimitiveBox("Kitchen_UpperCabinets", new Vector3(-2.0f, 2.0f, -2.2f), new Vector3(1.8f, 0.7f, 0.4f));
        upperCabinets.transform.SetParent(parent.transform, false);
    }

    private static void SetupZoneAnchor(GameObject parent, string name, Vector3 localPos)
    {
        GameObject anchor = new GameObject(name);
        anchor.transform.SetParent(parent.transform, false);
        anchor.transform.localPosition = localPos;
        anchor.transform.localRotation = Quaternion.identity;
        anchor.transform.localScale = Vector3.one;
    }

    private static GameObject CreatePrimitiveBox(string name, Vector3 localPos, Vector3 localScale)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.localPosition = localPos;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = localScale;

        if (box.GetComponent<BoxCollider>() == null)
        {
            box.AddComponent<BoxCollider>();
        }

        box.isStatic = true;
        return box;
    }
}
