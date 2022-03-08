using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{

    //properties -----------
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;

    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty resetTerrain;

    SerializedProperty voronoiFallOff;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiPeaks;
    SerializedProperty voronoiType;

    SerializedProperty MPDheightMin;
    SerializedProperty MPDheightMax;
    SerializedProperty MPDheightDampenerPower;
    SerializedProperty MPDroughness;

    SerializedProperty smoothPower;

    GUITableState splatMapTable;
    SerializedProperty splatHeights;
    SerializedProperty splatBlendBaseOffset;
    SerializedProperty splatBlendNoiseXOffset;
    SerializedProperty splatBlendNoiseYOffset;
    SerializedProperty splatBlendNoiseScaler;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;


    //fold outs ------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMidPointDisplacement = false;
    bool showSmoothTerrain = false;
    bool showSplatMaps = false;
    bool showHeights = false;

    Vector2 scrollPos;
    Texture2D hmtTexture;

    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");

        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");

        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiPeaks = serializedObject.FindProperty("voronoiPeaks");
        voronoiType = serializedObject.FindProperty("voronoiType");

        MPDheightMin = serializedObject.FindProperty("MPDheightMin");
        MPDheightMax = serializedObject.FindProperty("MPDheightMax");
        MPDheightDampenerPower = serializedObject.FindProperty("MPDheightDampenerPower");
        MPDroughness = serializedObject.FindProperty("MPDroughness");

        smoothPower = serializedObject.FindProperty("smoothPower");

        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");
        splatBlendBaseOffset = serializedObject.FindProperty("splatBlendBaseOffset");
        splatBlendNoiseXOffset = serializedObject.FindProperty("splatBlendNoiseXOffset");
        splatBlendNoiseYOffset = serializedObject.FindProperty("splatBlendNoiseYOffset");
        splatBlendNoiseScaler = serializedObject.FindProperty("splatBlendNoiseScaler");

        CustomTerrain terrain = (CustomTerrain)target;
        hmtTexture = new Texture2D(
            terrain.terrainData.heightmapResolution,
            terrain.terrainData.heightmapResolution,
            TextureFormat.ARGB32, false);
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        //Scrollbar Starting Code
        Rect r = EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;


        EditorGUILayout.PropertyField(resetTerrain);

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        //Show single perlin
        showPerlinNoise = EditorGUILayout.Foldout(showPerlinNoise, "Single Perlin Noise");
        if (showPerlinNoise)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, .02f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, .02f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("Offset X"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Offset Y"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));

            if (GUILayout.Button("Perlin"))
            {
                terrain.Perlin();
            }
        }

        //Show multiple perlin
        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if (showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable,
                perlinParameters);
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }
            if (GUILayout.Button("Perlin"))
            {
                terrain.Perlin();
            }
        }

        //Show Voronoi
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi)
        {
            EditorGUILayout.IntSlider(voronoiPeaks, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(voronoiFallOff, 0, 10, new GUIContent("FallOff"));
            EditorGUILayout.Slider(voronoiDropOff, 0, 10, new GUIContent("DropOff"));
            EditorGUILayout.Slider(voronoiMinHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);
            if (GUILayout.Button("Voronoi"))
            {
                terrain.Voronoi();
            }
        }

        //Show MidPointDisplacement
        showMidPointDisplacement = EditorGUILayout.Foldout(showMidPointDisplacement, "MidPointDisplacement");
        if (showMidPointDisplacement)
        {
            EditorGUILayout.Slider(MPDheightMin, -10, 10, new GUIContent("MPDheightMin"));
            EditorGUILayout.Slider(MPDheightMax, -10, 10, new GUIContent("MPDheightMax"));
            EditorGUILayout.Slider(MPDheightDampenerPower, 0, 10, new GUIContent("MPDheightDampenerPower"));
            EditorGUILayout.Slider(MPDroughness, 0, 10, new GUIContent("MPDroughness"));
            if (GUILayout.Button("MPD"))
            {
                terrain.MidPointDisplacement();
            }
        }

        //Show Splat Maps
        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps)
        {
            EditorGUILayout.Slider(splatBlendBaseOffset, 0, 1, new GUIContent("BlendBaseOffset"));
            EditorGUILayout.Slider(splatBlendNoiseXOffset, 0, 1, new GUIContent("BlendNoiseXOffset"));
            EditorGUILayout.Slider(splatBlendNoiseYOffset, 0, 1, new GUIContent("BlendNoiseYOffset"));
            EditorGUILayout.Slider(splatBlendNoiseScaler, 0, 1, new GUIContent("BlendNoiseScaler"));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
            splatMapTable = GUITableLayout.DrawTable(splatMapTable,
                splatHeights);
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply SplatMaps"))
            {
                terrain.SplatMaps();
            }
        }

        //Show SmoothTerrain
        showSmoothTerrain = EditorGUILayout.Foldout(showSmoothTerrain, "Smooth Terrain");
        if (showSmoothTerrain)
        {
            EditorGUILayout.Slider(smoothPower, 1, 100, new GUIContent("SmoothPower"));
            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }
        }

        showHeights = EditorGUILayout.Foldout(showHeights, "Height Map");
        if (showHeights)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int hmtSize = (int)(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(hmtTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(hmtSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0,
                    terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

                for (int y = 0; y < terrain.terrainData.heightmapResolution; ++y)
                {
                    for (int x = 0; x < terrain.terrainData.heightmapResolution; ++x)
                    {
                        hmtTexture.SetPixel(x, y, new Color(
                            heightMap[x, y],
                            heightMap[x, y],
                            heightMap[x, y],
                            1
                            ));
                    }
                }
                hmtTexture.Apply();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }


        //Scrollbar ending code
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
