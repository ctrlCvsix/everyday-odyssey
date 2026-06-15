using System.Collections.Generic;
using EverydayOdyssey;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SceneBootstrapper
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string KenneyRoot = "Assets/ThirdParty/Kenney";
    private const string CharacterRoot = KenneyRoot + "/BlockyCharacters";
    private const string UiRoot = KenneyRoot + "/UI";
    private const string AudioRoot = KenneyRoot + "/Audio";

    [MenuItem("Everyday Odyssey/Build Prototype Scene")]
    public static void BuildPrototypeScene()
    {
        EnsureFolders();
        PrepareUiAssets();

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientLight = new Color(0.72f, 0.74f, 0.78f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.76f, 0.79f, 0.82f);
        RenderSettings.fogDensity = 0.0035f;

        Material asphalt = CreateMaterial("Asphalt", new Color(0.23f, 0.25f, 0.27f));
        Material sidewalk = CreateMaterial("Sidewalk", new Color(0.58f, 0.58f, 0.58f));
        Material brick = CreateMaterial("Brick", new Color(0.58f, 0.24f, 0.18f));
        Material grass = CreateMaterial("Grass", new Color(0.37f, 0.47f, 0.24f));
        Material pillarMat = CreateMaterial("PillarBlack", new Color(0.06f, 0.06f, 0.07f));
        Material rock = CreateMaterial("RockDark", new Color(0.23f, 0.23f, 0.25f));
        Material fence = CreateMaterial("Fence", new Color(0.12f, 0.12f, 0.12f));
        Material code = CreateMaterial("CodeCyan", new Color(0.18f, 0.95f, 1f));
        Material building = CreateMaterial("Building", new Color(0.78f, 0.81f, 0.84f));
        Material glass = CreateMaterial("Glass", new Color(0.42f, 0.7f, 0.78f));
        Material adFallback = CreateMaterial("AdFallback", new Color(0.95f, 0.9f, 0.2f));

        CreateLight();
        CreateGround(asphalt, sidewalk, brick, grass, rock, fence);
        CreateCampusLandmarks(pillarMat, building, glass, brick, grass, rock);
        CreateBillboard(adFallback);
        CreateBoundaries();
        CreateEventSystem();

        GameObject systems = new GameObject("Systems");
        PrototypeAudioBank audioBank = systems.AddComponent<PrototypeAudioBank>();
        audioBank.Configure(
            LoadAsset<AudioClip>($"{AudioRoot}/Interface/click_003.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Interface/confirmation_002.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Interface/error_004.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Interface/glitch_003.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Impact/impactMetal_light_003.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Interface/confirmation_002.ogg"),
            LoadAsset<AudioClip>("Assets/Imported/gunshot_real.mp3"),
            LoadAsset<AudioClip>($"{AudioRoot}/Impact/footstep_concrete_001.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Impact/footstep_concrete_003.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Impact/footstep_grass_001.ogg"),
            LoadAsset<AudioClip>($"{AudioRoot}/Impact/footstep_grass_003.ogg"));

        GameManager manager = systems.AddComponent<GameManager>();
        Camera camera = CreateCamera();
        PlayerController player = CreatePlayer(camera, code);
        CreateUploadZone();
        List<InteractableTerminal> terminals = CreateTeacherAndTerminalSet();
        BuildUI(manager, player, audioBank);

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prototype scene created with {terminals.Count} mentor terminals and upload gate.");
    }

    public static void BuildPrototypeSceneFromBatch()
    {
        BuildPrototypeScene();
        EditorApplication.Exit(0);
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets")) AssetDatabase.CreateFolder("Assets", "StreamingAssets");
    }

    private static void PrepareUiAssets()
    {
        SetTextureAsSprite($"{UiRoot}/button_rectangle_depth_gloss.png");
        SetTextureAsSprite($"{UiRoot}/button_rectangle_depth_flat.png");
        SetTextureAsSprite($"{UiRoot}/button_square_depth_flat.png");
    }

    private static void SetTextureAsSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        material.mainTexture = null;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static T LoadAsset<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.22f;
        light.color = new Color(0.97f, 0.96f, 0.92f);
        light.transform.rotation = Quaternion.Euler(38f, -26f, 0f);
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 900f;
        camera.fieldOfView = 60f;
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static void CreateGround(Material asphalt, Material sidewalk, Material brick, Material grass, Material rock, Material fence)
    {
        CreateCube("CampusSupport", new Vector3(0f, -1.2f, 70f), new Vector3(360f, 2f, 440f), CreateMaterial("SupportDark", new Color(0.18f, 0.18f, 0.2f)));
        CreateCube("RoadBase", new Vector3(0f, -0.05f, -64f), new Vector3(96f, 0.1f, 42f), asphalt);
        CreateCube("CrosswalkBase", new Vector3(0f, 0.02f, -62f), new Vector3(42f, 0.03f, 16f), CreateMaterial("CrosswalkWhite", new Color(0.92f, 0.92f, 0.92f)));
        for (int i = -10; i <= 10; i += 2)
        {
            CreateCube($"CrosswalkGap_{i}", new Vector3(i * 2f, 0.03f, -62f), new Vector3(1.1f, 0.04f, 16f), asphalt);
        }

        CreateCube("SidewalkFront", new Vector3(0f, 0.16f, -34f), new Vector3(104f, 0.32f, 14f), sidewalk);
        CreateCube("GateApron", new Vector3(0f, 0.06f, 6f), new Vector3(34f, 0.12f, 26f), brick);
        CreateCube("MainBrickRoad", new Vector3(0f, 0f, 54f), new Vector3(70f, 0.12f, 184f), brick);

        CreateCube("LowerLawnLeft", new Vector3(-52f, 0.45f, 44f), new Vector3(58f, 0.9f, 102f), grass);
        CreateCube("LowerLawnRight", new Vector3(52f, 0.45f, 48f), new Vector3(58f, 0.9f, 108f), grass);
        CreateCube("UpperLawnLeft", new Vector3(-44f, 0.9f, 128f), new Vector3(52f, 0.7f, 74f), grass);
        CreateCube("UpperLawnRight", new Vector3(44f, 0.9f, 132f), new Vector3(52f, 0.7f, 74f), grass);
        CreateCube("RearLawn", new Vector3(0f, 0.5f, 194f), new Vector3(128f, 1f, 34f), grass);

        CreateCube("PlatformConnector", new Vector3(0f, 0.55f, 108f), new Vector3(82f, 0.22f, 22f), brick);
        CreateRamp("LeftBroadRamp", new Vector3(-36f, 0.62f, 99f), new Vector3(34f, 0.4f, 34f), -3.5f, brick);
        CreateRamp("RightBroadRamp", new Vector3(36f, 0.62f, 99f), new Vector3(34f, 0.4f, 34f), -3.5f, brick);
        CreateRamp("CenterBroadRamp", new Vector3(0f, 0.5f, 100f), new Vector3(36f, 0.35f, 36f), -3.5f, brick);
        CreateCube("LeftNoGapWalkway", new Vector3(-42f, 0.72f, 116f), new Vector3(42f, 0.18f, 30f), brick);
        CreateCube("RightNoGapWalkway", new Vector3(42f, 0.72f, 116f), new Vector3(42f, 0.18f, 30f), brick);

        for (int i = 0; i < 20; i++)
        {
            float z = -8f + i * 8f;
            CreateCube($"FencePostLeft_{i}", new Vector3(-36f, 0.75f, z), new Vector3(0.2f, 1.35f, 0.2f), fence);
            CreateCube($"FencePostRight_{i}", new Vector3(36f, 0.75f, z), new Vector3(0.2f, 1.35f, 0.2f), fence);
        }
        CreateCube("FenceRailLeftTop", new Vector3(-36f, 1.1f, 68f), new Vector3(0.12f, 0.08f, 156f), fence);
        CreateCube("FenceRailLeftMid", new Vector3(-36f, 0.55f, 68f), new Vector3(0.12f, 0.08f, 156f), fence);
        CreateCube("FenceRailRightTop", new Vector3(36f, 1.1f, 68f), new Vector3(0.12f, 0.08f, 156f), fence);
        CreateCube("FenceRailRightMid", new Vector3(36f, 0.55f, 68f), new Vector3(0.12f, 0.08f, 156f), fence);
    }

    private static void CreateCampusLandmarks(Material pillarMat, Material building, Material glass, Material brick, Material grass, Material rock)
    {
        GameObject leftPillar = CreateCube("TwinPillarLeft", new Vector3(-10f, 7f, 4f), new Vector3(1.4f, 14f, 1.4f), pillarMat);
        GameObject rightPillar = CreateCube("TwinPillarRight", new Vector3(10f, 7f, 4f), new Vector3(1.4f, 14f, 1.4f), pillarMat);
        AddLetterStrip(leftPillar.transform);
        AddLetterStrip(rightPillar.transform);

        CreateBuildingBlock("AdminHall", new Vector3(-74f, 9f, 34f), new Vector3(30f, 18f, 18f), building, glass);
        CreateBuildingBlock("LibraryWing", new Vector3(78f, 12f, 38f), new Vector3(34f, 24f, 22f), building, glass);
        CreateBuildingBlock("ResearchCenter", new Vector3(0f, 12f, 198f), new Vector3(72f, 24f, 18f), building, glass);
        CreateBuildingBlock("StudentCenter", new Vector3(-78f, 10f, 142f), new Vector3(26f, 20f, 18f), building, glass);
        CreateBuildingBlock("DesignBlock", new Vector3(78f, 10f, 144f), new Vector3(26f, 20f, 18f), building, glass);

        CreateCube("ScreenPlaza", new Vector3(0f, 0.2f, 148f), new Vector3(34f, 0.4f, 24f), brick);
        CreateCube("BenchLeft", new Vector3(-14f, 0.7f, 152f), new Vector3(8f, 1.2f, 2f), CreateMaterial("BenchWood", new Color(0.48f, 0.32f, 0.18f)));
        CreateCube("BenchRight", new Vector3(14f, 0.7f, 152f), new Vector3(8f, 1.2f, 2f), CreateMaterial("BenchWood", new Color(0.48f, 0.32f, 0.18f)));

        CreateTree(new Vector3(-62f, 0f, 28f), 1.0f);
        CreateTree(new Vector3(58f, 0f, 28f), 1.0f);
        CreateTree(new Vector3(-52f, 0f, 86f), 1.1f);
        CreateTree(new Vector3(54f, 0f, 88f), 1.1f);
        CreateTree(new Vector3(-42f, 1.2f, 132f), 1.15f);
        CreateTree(new Vector3(42f, 1.2f, 136f), 1.15f);
        CreateTree(new Vector3(-12f, 0f, 184f), 1.2f);
        CreateTree(new Vector3(12f, 0f, 186f), 1.2f);

        CreateCube("StoneSculptureA", new Vector3(46f, 2.2f, 58f), new Vector3(5f, 1.8f, 3f), rock);
        CreateCube("StoneSculptureB", new Vector3(54f, 2.3f, 61f), new Vector3(4.2f, 2f, 2.6f), rock);
    }

    private static void CreateBillboard(Material fallbackMaterial)
    {
        Material frame = CreateMaterial("BillboardFrame", new Color(0.1f, 0.1f, 0.11f));
        GameObject parent = new GameObject("AdBillboard");
        parent.transform.position = new Vector3(0f, 0f, 156f);

        CreateChildCube("LeftPole", parent.transform, new Vector3(-10f, 10f, 0f), new Vector3(1f, 20f, 1f), frame);
        CreateChildCube("RightPole", parent.transform, new Vector3(10f, 10f, 0f), new Vector3(1f, 20f, 1f), frame);
        CreateChildCube("Frame", parent.transform, new Vector3(0f, 18f, 0f), new Vector3(24f, 14f, 1f), frame);

        GameObject frontScreen = CreateChildQuad("FrontScreen", parent.transform, new Vector3(0f, 18f, 0.56f), new Vector3(20f, 10f, 1f), fallbackMaterial);
        GameObject backScreen = CreateChildQuad("BackScreen", parent.transform, new Vector3(0f, 18f, -0.56f), new Vector3(20f, 10f, 1f), fallbackMaterial);
        backScreen.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        BillboardAdScreen frontAd = frontScreen.AddComponent<BillboardAdScreen>();
        frontAd.Configure(frontScreen.GetComponent<MeshRenderer>(), fallbackMaterial, true);
        BillboardAdScreen backAd = backScreen.AddComponent<BillboardAdScreen>();
        backAd.Configure(backScreen.GetComponent<MeshRenderer>(), fallbackMaterial, false);
    }

    private static void CreateBuildingBlock(string name, Vector3 center, Vector3 size, Material wall, Material glass)
    {
        GameObject building = CreateCube(name, center, size, wall);
        for (int row = -2; row <= 2; row++)
        {
            for (int col = -3; col <= 3; col++)
            {
                CreateChildCube(
                    $"Window_{row}_{col}",
                    building.transform,
                    new Vector3(col * (size.x / 8f), row * (size.y / 7f), (size.z * 0.5f) + 0.02f),
                    new Vector3(size.x / 12f, size.y / 10f, 0.08f),
                    glass);
            }
        }
    }

    private static void CreateTree(Vector3 position, float scale)
    {
        Material bark = CreateMaterial("Bark", new Color(0.29f, 0.18f, 0.11f));
        Material leaves = CreateMaterial("Leaves", new Color(0.24f, 0.41f, 0.19f));
        float trunkHeight = 4.2f * scale;
        float crownSize = 3.6f * scale;
        CreateCube($"TreeTrunk_{position.x}_{position.z}", position + new Vector3(0f, trunkHeight * 0.5f, 0f), new Vector3(0.55f * scale, trunkHeight, 0.55f * scale), bark);
        CreateCube($"TreeCrown_{position.x}_{position.z}", position + new Vector3(0f, trunkHeight + (crownSize * 0.45f), 0f), new Vector3(crownSize, crownSize, crownSize), leaves);
    }

    private static void CreateRamp(string name, Vector3 position, Vector3 size, float pitchDegrees, Material material)
    {
        GameObject ramp = CreateCube(name, position, size, material);
        ramp.transform.rotation = Quaternion.Euler(pitchDegrees, 0f, 0f);
    }

    private static void CreateStaircase(string name, Vector3 origin, int steps, float width, float depth, float stepHeight, Material material)
    {
        GameObject root = new GameObject(name);
        for (int i = 0; i < steps; i++)
        {
            float y = origin.y + (stepHeight * 0.5f) + (i * stepHeight);
            float z = origin.z + (i * depth);
            CreateChildCube($"Step_{i}", root.transform, new Vector3(0f, y, z), new Vector3(width, stepHeight, depth + 0.05f), material);
        }
    }

    private static void AddLetterStrip(Transform pillar)
    {
        CreateChildCube($"Label_{pillar.name}", pillar, new Vector3(0.36f, 0f, 0.72f), new Vector3(0.1f, 9f, 0.03f), CreateMaterial("LetterStrip", new Color(0.8f, 0.8f, 0.8f)));
    }

    private static void CreateBoundaries()
    {
        Material invisible = CreateMaterial("InvisibleBoundary", new Color(1f, 1f, 1f, 0f));
        CreateInvisibleCube("FailsafeFloor", new Vector3(0f, -8f, 70f), new Vector3(420f, 1f, 500f), invisible);
        CreateInvisibleCube("BoundaryNorth", new Vector3(0f, 4f, 238f), new Vector3(190f, 8f, 1f), invisible);
        CreateInvisibleCube("BoundarySouth", new Vector3(0f, 4f, -96f), new Vector3(190f, 8f, 1f), invisible);
        CreateInvisibleCube("BoundaryEast", new Vector3(124f, 4f, 70f), new Vector3(1f, 8f, 340f), invisible);
        CreateInvisibleCube("BoundaryWest", new Vector3(-124f, 4f, 70f), new Vector3(1f, 8f, 340f), invisible);
    }

    private static PlayerController CreatePlayer(Camera sceneCamera, Material laptopMat)
    {
        GameObject root = new GameObject("Player");
        root.transform.position = new Vector3(0f, 1.1f, -42f);

        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 1.7f;
        controller.radius = 0.28f;
        controller.center = new Vector3(0f, 0.85f, 0f);

        CharacterMotionController animatorRig = BuildImportedCharacterVisual(root.transform, "character-a", false, new Color(0.18f, 0.42f, 0.88f), laptopMat, false);

        GameObject firstPersonMount = new GameObject("FirstPersonMount");
        firstPersonMount.transform.SetParent(root.transform, false);
        firstPersonMount.transform.localPosition = new Vector3(0f, 1.48f, 0.04f);

        GameObject thirdPersonMount = new GameObject("ThirdPersonMount");
        thirdPersonMount.transform.SetParent(root.transform, false);
        thirdPersonMount.transform.localPosition = new Vector3(0f, 1.5f, -2.1f);

        GameObject attackOrigin = new GameObject("AttackOrigin");
        attackOrigin.transform.SetParent(root.transform, false);
        attackOrigin.transform.localPosition = new Vector3(0.18f, 1.02f, 0.56f);

        PlayerController playerController = root.AddComponent<PlayerController>();
        playerController.Configure(sceneCamera, firstPersonMount.transform, thirdPersonMount.transform, attackOrigin.transform, animatorRig);
        return playerController;
    }

    private static UploadZone CreateUploadZone()
    {
        GameObject zone = CreateCube("UploadZone", new Vector3(0f, 0.45f, 6f), new Vector3(14f, 0.9f, 10f), CreateMaterial("UploadZone", new Color(0.12f, 0.75f, 0.95f, 0.18f)));
        zone.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return zone.AddComponent<UploadZone>();
    }

    private static List<InteractableTerminal> CreateTeacherAndTerminalSet()
    {
        string[] teacherNamesZh = { "AI导师", "建模导师", "图形导师", "系统导师", "设计导师" };
        string[] teacherNamesKo = { "AI 멘토", "모델링 멘토", "그래픽 멘토", "시스템 멘토", "디자인 멘토" };
        string[] terminalNamesZh = { "AI评审终端", "建模评审终端", "图形评审终端", "系统评审终端", "设计评审终端" };
        string[] terminalNamesKo = { "AI 평가 터미널", "모델링 평가 터미널", "그래픽 평가 터미널", "시스템 평가 터미널", "디자인 평가 터미널" };
        string[] teacherModels = { "character-f", "character-h", "character-k", "character-p", "character-c" };
        Vector3[] teacherPositions =
        {
            new Vector3(-54f, 1f, 52f),
            new Vector3(54f, 1f, 56f),
            new Vector3(-44f, 1f, 136f),
            new Vector3(44f, 1f, 140f),
            new Vector3(0f, 1f, 184f)
        };
        Vector3[] terminalPositions =
        {
            new Vector3(-70f, 1f, 68f),
            new Vector3(70f, 1f, 72f),
            new Vector3(-28f, 1f, 152f),
            new Vector3(28f, 1f, 156f),
            new Vector3(0f, 1f, 116f)
        };
        Color[] colors =
        {
            new Color(0.92f, 0.38f, 0.38f),
            new Color(0.38f, 0.8f, 0.52f),
            new Color(0.36f, 0.56f, 0.95f),
            new Color(0.93f, 0.78f, 0.27f),
            new Color(0.81f, 0.39f, 0.92f)
        };

        List<InteractableTerminal> terminals = new List<InteractableTerminal>();
        GameObject parent = new GameObject("MentorSet");

        for (int i = 0; i < teacherNamesZh.Length; i++)
        {
            GameObject teacherRoot = new GameObject($"Teacher_{i + 1}");
            teacherRoot.transform.SetParent(parent.transform);
            teacherRoot.transform.position = teacherPositions[i];

            CharacterController cc = teacherRoot.AddComponent<CharacterController>();
            cc.height = 1.6f;
            cc.radius = 0.26f;
            cc.center = new Vector3(0f, 0.8f, 0f);

            CharacterMotionController motion = BuildImportedCharacterVisual(teacherRoot.transform, teacherModels[i], true, colors[i], CreateMaterial($"TeacherTablet_{i}", Color.Lerp(colors[i], Color.white, 0.25f)), true);
            Transform[] patrolPoints =
            {
                CreateWaypoint($"Teacher_{i + 1}_P1", teacherPositions[i] + new Vector3(-8f, 0f, 8f), parent.transform),
                CreateWaypoint($"Teacher_{i + 1}_P2", teacherPositions[i] + new Vector3(8f, 0f, -8f), parent.transform),
                CreateWaypoint($"Teacher_{i + 1}_P3", terminalPositions[i] + new Vector3(0f, 0f, 8f), parent.transform)
            };

            TeacherAI teacher = teacherRoot.AddComponent<TeacherAI>();
            teacher.Configure(teacherNamesZh[i], teacherNamesKo[i], patrolPoints, motion);
            teacher.SetFlashRenderers(teacherRoot.GetComponentsInChildren<Renderer>());

            GameObject terminalRoot = CreateCube($"Terminal_{i + 1}", terminalPositions[i], new Vector3(2.4f, 2.2f, 2.4f), CreateMaterial($"TerminalBase_{i}", new Color(0.15f, 0.15f, 0.17f)));
            terminalRoot.transform.SetParent(parent.transform);
            GameObject beacon = CreateCube($"Terminal_{i + 1}_Beacon", terminalPositions[i] + new Vector3(0f, 1.8f, 0f), new Vector3(0.8f, 0.8f, 0.8f), CreateMaterial($"TerminalBeacon_{i}", colors[i]));
            beacon.transform.SetParent(terminalRoot.transform);

            GameObject fragmentRoot = new GameObject($"Fragment_{i + 1}");
            fragmentRoot.transform.SetParent(terminalRoot.transform, false);
            fragmentRoot.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            GameObject fragment = CreateChildCube("FragmentVisual", fragmentRoot.transform, Vector3.zero, new Vector3(0.75f, 0.75f, 0.75f), CreateMaterial($"FragmentMat_{i}", Color.Lerp(colors[i], Color.white, 0.35f)));
            fragment.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);
            fragment.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            InteractableTerminal terminal = terminalRoot.AddComponent<InteractableTerminal>();
            terminal.Configure(i, terminalNamesZh[i], terminalNamesKo[i], beacon.GetComponent<MeshRenderer>(), fragment.GetComponent<Renderer>(), fragmentRoot);
            terminals.Add(terminal);
        }

        return terminals;
    }

    private static Transform CreateWaypoint(string name, Vector3 position, Transform parent)
    {
        GameObject point = new GameObject(name);
        point.transform.SetParent(parent);
        point.transform.position = position;
        return point.transform;
    }

    private static void BuildUI(GameManager manager, PlayerController player, PrototypeAudioBank audioBank)
    {
        Font uiFont = LoadAsset<Font>($"{UiRoot}/Kenney Future.ttf") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite largeButtonSprite = LoadAsset<Sprite>($"{UiRoot}/button_rectangle_depth_gloss.png");
        Sprite smallButtonSprite = LoadAsset<Sprite>($"{UiRoot}/button_rectangle_depth_flat.png");

        GameObject canvasObject = new GameObject("HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        CreateOverlayPanel("TopLeftPanel", canvas.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(760f, 92f), new Color(0.05f, 0.11f, 0.14f, 0.72f));
        CreateOverlayPanel("TopCenterPanel", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(260f, 72f), new Color(0.05f, 0.11f, 0.14f, 0.72f));
        CreateOverlayPanel("TopRightPanel", canvas.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(260f, 102f), new Color(0.05f, 0.11f, 0.14f, 0.72f));
        CreateOverlayPanel("BottomPanel", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(1100f, 110f), new Color(0.05f, 0.11f, 0.14f, 0.72f));

        Text objective = CreateText("Objective", canvas.transform, uiFont, new Vector2(20f, -20f), new Vector2(760f, 92f), 24, TextAnchor.UpperLeft);
        Text timer = CreateAnchoredText("Timer", canvas.transform, uiFont, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(260f, 72f), 26, TextAnchor.MiddleCenter);
        Text focus = CreateAnchoredText("Focus", canvas.transform, uiFont, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(260f, 102f), 20, TextAnchor.MiddleCenter);
        Text prompt = CreateAnchoredText("Prompt", canvas.transform, uiFont, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1020f, 42f), 20, TextAnchor.LowerCenter);
        Text status = CreateAnchoredText("Status", canvas.transform, uiFont, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 72f), new Vector2(1050f, 36f), 18, TextAnchor.LowerCenter);

        Button floatingChinese = CreateButton("FloatingChinese", canvas.transform, largeButtonSprite, uiFont, "中文", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-210f, 24f), new Vector2(92f, 40f), out Text floatingChineseText);
        Button floatingKorean = CreateButton("FloatingKorean", canvas.transform, largeButtonSprite, uiFont, "한국어", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-104f, 24f), new Vector2(92f, 40f), out Text floatingKoreanText);

        GameObject tutorialRoot = new GameObject("TutorialRoot");
        tutorialRoot.transform.SetParent(canvas.transform, false);
        Image dimmer = tutorialRoot.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform dimRect = dimmer.rectTransform;
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;

        Image tutorialPanel = CreateOverlayPanel("TutorialPanel", tutorialRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920f, 620f), new Color(0.06f, 0.16f, 0.2f, 0.95f));
        Text title = CreateAnchoredText("Title", tutorialRoot.transform, uiFont, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(720f, 60f), 34, TextAnchor.MiddleCenter);
        Text tutorialBody = CreateAnchoredText("TutorialBody", tutorialRoot.transform, uiFont, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(760f, 320f), 22, TextAnchor.UpperLeft);
        Text languageLabel = CreateAnchoredText("LanguageLabel", tutorialRoot.transform, uiFont, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(320f, 36f), 20, TextAnchor.MiddleCenter);
        Button chineseButton = CreateButton("ChineseButton", tutorialRoot.transform, smallButtonSprite, uiFont, "中文", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-90f, -180f), new Vector2(150f, 52f), out Text chineseButtonText);
        Button koreanButton = CreateButton("KoreanButton", tutorialRoot.transform, smallButtonSprite, uiFont, "한국어", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(90f, -180f), new Vector2(150f, 52f), out Text koreanButtonText);
        Button startButton = CreateButton("StartButton", tutorialRoot.transform, largeButtonSprite, uiFont, "开始游戏", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(260f, 64f), out Text startButtonText);

        GameObject loseRoot = new GameObject("LoseRoot");
        loseRoot.transform.SetParent(canvas.transform, false);
        loseRoot.SetActive(false);
        Image loseDimmer = loseRoot.AddComponent<Image>();
        loseDimmer.color = new Color(0f, 0f, 0f, 0.74f);
        RectTransform loseDimRect = loseDimmer.rectTransform;
        loseDimRect.anchorMin = Vector2.zero;
        loseDimRect.anchorMax = Vector2.one;
        loseDimRect.offsetMin = Vector2.zero;
        loseDimRect.offsetMax = Vector2.zero;
        Image losePanel = CreateOverlayPanel("LosePanel", loseRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 300f), new Color(0.18f, 0.08f, 0.08f, 0.95f));
        Text loseTitle = CreateAnchoredText("LoseTitle", loseRoot.transform, uiFont, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 72f), new Vector2(420f, 52f), 32, TextAnchor.MiddleCenter);
        Text loseBody = CreateAnchoredText("LoseBody", loseRoot.transform, uiFont, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(500f, 80f), 20, TextAnchor.MiddleCenter);
        Button restartButton = CreateButton("RestartButton", loseRoot.transform, largeButtonSprite, uiFont, "重新开始", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -86f), new Vector2(220f, 58f), out Text restartButtonText);

        manager.BindUI(objective, timer, focus, prompt, status, title, tutorialBody, languageLabel, startButtonText, chineseButtonText, koreanButtonText, floatingChineseText, floatingKoreanText, loseTitle, loseBody, restartButtonText, tutorialPanel, losePanel, startButton, chineseButton, koreanButton, floatingChinese, floatingKorean, restartButton, audioBank, player);
    }

    private static CharacterMotionController BuildImportedCharacterVisual(Transform parent, string modelName, bool teacherStyle, Color bodyTint, Material deviceMaterial, bool includeHeldDevice)
    {
        GameObject prefab = LoadAsset<GameObject>($"{CharacterRoot}/FBX/{modelName}.fbx");
        GameObject container = new GameObject("Model");
        container.transform.SetParent(parent, false);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        container.transform.localScale = Vector3.one * 0.6f;

        if (prefab != null)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.SetParent(container.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            ApplySharedMaterial(instance, CreateMaterial($"Character_{modelName}_{bodyTint.r:0.00}_{bodyTint.g:0.00}_{bodyTint.b:0.00}", bodyTint));
            AlignCharacterToGround(instance, parent.position.y);
        }

        if (includeHeldDevice)
        {
            CreateChildCube(
                teacherStyle ? "Tablet" : "Laptop",
                container.transform,
                teacherStyle ? new Vector3(0.18f, 0.95f, 0.16f) : new Vector3(0.26f, 0.88f, 0.26f),
                teacherStyle ? new Vector3(0.28f, 0.08f, 0.18f) : new Vector3(0.42f, 0.08f, 0.28f),
                deviceMaterial);
        }

        SimpleModelAnimator animator = container.AddComponent<SimpleModelAnimator>();
        animator.Configure(container.transform);
        return animator;
    }

    private static void AlignCharacterToGround(GameObject instance, float targetGroundY)
    {
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float delta = targetGroundY - bounds.min.y;
        instance.transform.position += Vector3.up * (delta + 0.02f);
    }

    private static Text CreateText(string name, Transform parent, Font font, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor anchor)
    {
        return CreateAnchoredText(name, parent, font, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size, fontSize, anchor);
    }

    private static Text CreateAnchoredText(string name, Transform parent, Font font, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMax.x, anchorMax.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return text;
    }

    private static Image CreateOverlayPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.AddComponent<Image>();
        image.color = color;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMax.x, anchorMax.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return image;
    }

    private static Button CreateButton(string name, Transform parent, Sprite sprite, Font font, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, out Text text)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = image.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMax.x, anchorMax.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        text = CreateAnchoredText($"{name}_Text", buttonObject.transform, font, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 20, TextAnchor.MiddleCenter);
        text.text = label;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 16;
        text.resizeTextMaxSize = 24;
        return button;
    }

    private static void ApplySharedMaterial(GameObject root, Material material)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                renderer.sharedMaterial = material;
                continue;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.sharedMaterials = materials;
        }
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static GameObject CreateInvisibleCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = CreateCube(name, position, scale, material);
        cube.GetComponent<Renderer>().enabled = false;
        return cube;
    }

    private static GameObject CreateChildCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = localScale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static GameObject CreateChildQuad(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.SetParent(parent, false);
        quad.transform.localPosition = localPosition;
        quad.transform.localScale = localScale;
        quad.GetComponent<Renderer>().sharedMaterial = material;
        return quad;
    }
}
