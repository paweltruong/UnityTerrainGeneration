using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool resetTerrain = true;

    //PERLIN NOISE
    [Range(0, 0.02f)]
    public float perlinXScale = 0.01f;
    [Range(0, 0.02f)]
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;

    //MULTIPLE PERLIN
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>() { new PerlinParameters() };

    //Splatmaps
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1.5f;
        public Vector2 tileOffset = Vector2.zero;
        public Vector2 tileSize = new Vector2(50, 50);
        public bool overrideGlobalBlendSettings = false;
        public float splatBlendBaseOffset = 0f;
        public float splatBlendNoiseXOffset = 0.01f;
        public float splatBlendNoiseYOffset = 0.01f;
        public float splatBlendNoiseScaler = 0.01f;
        public bool remove = false;
    }
    public List<SplatHeights> splatHeights = new List<SplatHeights>() { new SplatHeights() };
    public float splatBlendBaseOffset = 0f;
    public float splatBlendNoiseXOffset = 0.01f;
    public float splatBlendNoiseYOffset = 0.01f;
    public float splatBlendNoiseScaler = 0.01f;

    //Voronoi
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMinHeight = 0.25f;
    public float voronoiMaxHeight = 0.4f;
    public int voronoiPeaks = 9;
    public enum VoronoiType
    {
        Linear = 0, Power = 1, Combined = 2,
        SinPow = 3
    }
    public VoronoiType voronoiType = VoronoiType.Linear;

    //Midpoint Displacement
    public float MPDheightMin = -2f;
    public float MPDheightMax = 2f;
    public float MPDheightDampenerPower = 2.0f;
    public float MPDroughness = 2.0f;

    //Smooth
    public float smoothPower = 10f;

    public Terrain terrain;
    public TerrainData terrainData;

    float[,] GetHeightMap(bool ignoreResetTerrain = false)
    {
        if (!resetTerrain || ignoreResetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        }
        else
        {
            return new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        }
    }

    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        var keptSplatHeights = new List<SplatHeights>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptSplatHeights.Add(splatHeights[i]);
            }
        }

        if (keptSplatHeights.Count == 0) //don't want to keep any
        {
            keptSplatHeights.Add(splatHeights[0]);//add at least 1
        }
        splatHeights = keptSplatHeights;
    }

    float GetSteepness(float[,] heightmap, int x, int y, int width, int height)
    {
        //Sobel Edge Detection
        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        //if on the upper edge of the map find gradient by going backward
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightmap[nx, y] - h;
        float dy = heightmap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);
        float steep = gradient.magnitude;
        return steep;
    }

    public void SplatMaps()
    {
        //Create terrain brushes
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int splatIndex = 0;
        foreach (var sh in splatHeights)
        {
            newSplatPrototypes[splatIndex] = new TerrainLayer();
            newSplatPrototypes[splatIndex].diffuseTexture = sh.texture;
            newSplatPrototypes[splatIndex].tileOffset = sh.tileOffset;
            newSplatPrototypes[splatIndex].tileSize = sh.tileSize;
            newSplatPrototypes[splatIndex].diffuseTexture.Apply(true);

            splatIndex++;
        }
        terrainData.terrainLayers = newSplatPrototypes;


        //Apply texture based on height
        var heightMap = GetHeightMap(true);
        float[,,] splatMapData = new float[
            terrainData.alphamapWidth,
            terrainData.alphamapHeight,
            terrainData.alphamapLayers
            ];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeights.Count; ++i)
                {
                    //set global blend
                    float noise = Mathf.PerlinNoise(x * splatBlendNoiseXOffset, y * splatBlendNoiseYOffset) * splatBlendNoiseScaler;
                    float blendOffset = splatBlendBaseOffset + noise;

                    //set custom blend
                    if (splatHeights[i].overrideGlobalBlendSettings)
                    {
                        noise = Mathf.PerlinNoise(x * splatHeights[i].splatBlendNoiseXOffset,
                            y * splatHeights[i].splatBlendNoiseYOffset) * splatHeights[i].splatBlendNoiseScaler;
                        blendOffset = splatHeights[i].splatBlendBaseOffset + noise;
                    }

                    float thisHeightStart = splatHeights[i].minHeight - blendOffset;
                    float thisHeightStop = splatHeights[i].maxHeight + blendOffset;

                    //Sobel
                    //float steepness = GetSteepness(heightMap, x, y, terrainData.heightmapResolution, terrainData.heightmapResolution);
                    
                    //normalized (0,1) and need to swap x and y because unity alpha map x,y are rotated 90 degrees (behaves differently on heightmaps and splatmaps in unity)
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight,
                        x / (float)terrainData.alphamapWidth);

                    bool isSplatSteepnessValid = steepness >= splatHeights[i].minSlope
                        && steepness <= splatHeights[i].maxSlope;

                    if (heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop
                        && isSplatSteepnessValid)
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; ++j)
                {
                    splatMapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    void NormalizeVector(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; ++i)
        {
            total += v[i];
        }
        for (int i = 0; i < v.Length; ++i)
        {
            v[i] /= total;
        }
    }

    public void Smooth()
    {
        var heightMap = GetHeightMap(true);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < smoothPower; ++i)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    float averageHeight = heightMap[x, y];

                    var neighbours = GenereateNeighbours(new Vector2(x, y),
                        terrainData.heightmapResolution, terrainData.heightmapResolution);

                    foreach (var n in neighbours)
                    {
                        averageHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = averageHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / smoothPower);
        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    List<Vector2> GenereateNeighbours(Vector2 pos, int width, int height)
    {
        var neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1), Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(nPos))
                        neighbours.Add(nPos);
                }
            }
        }
        return neighbours;
    }

    public void MidPointDisplacement()
    {
        var heightMap = GetHeightMap();
        int width = terrainData.heightmapResolution - 1;
        int squareSize = width;
        float heightMin = MPDheightMin;
        float heightMax = MPDheightMax;
        float heightDampener = (float)Mathf.Pow(MPDheightDampenerPower, -1 * MPDroughness);

        int cornerX, cornerY;
        int midX, midY;
        int pMidXL, pMidXR, pMidYU, pMidYD;


        heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[0, terrainData.alphamapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapResolution - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapResolution - 2, terrainData.heightmapResolution - 2] = UnityEngine.Random.Range(0f, 0.2f);

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;
                    midX = (int)(x + squareSize / 2f);
                    midY = (int)(y + squareSize / 2f);

                    heightMap[midX, midY] = (float)(
                        (heightMap[x, y] + heightMap[cornerX, y] + heightMap[x, cornerY] + heightMap[cornerX, cornerY]) / 4.0f
                        + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;
                    midX = (int)(x + squareSize / 2f);
                    midY = (int)(y + squareSize / 2f);

                    pMidXR = (int)(midX + squareSize);
                    pMidYU = (int)(midY + squareSize);
                    pMidXL = (int)(midX - squareSize);
                    pMidYD = (int)(midY - squareSize);

                    if (pMidXL <= 0 || pMidYD <= 0 || pMidXR >= width - 1 || pMidYU >= width - 1) continue;

                    //left
                    heightMap[midX, y] = CalculateMidPointHeight(
                        left: heightMap[pMidXL, midY],
                        top: heightMap[x, cornerY],
                        right: heightMap[midX, midY],
                        bottom: heightMap[x, y],
                        heightMin: heightMin,
                        heightMax: heightMax);
                    //top
                    heightMap[midX, cornerY] = CalculateMidPointHeight(
                        left: heightMap[x, cornerY],
                        top: heightMap[midX, pMidYU],
                        right: heightMap[cornerX, cornerY],
                        bottom: heightMap[midX, midY],
                        heightMin: heightMin,
                        heightMax: heightMax);
                    //right
                    heightMap[cornerX, midY] = CalculateMidPointHeight(
                        left: heightMap[midX, midY],
                        top: heightMap[cornerX, cornerY],
                        right: heightMap[pMidXR, midY],
                        bottom: heightMap[cornerX, y],
                        heightMin: heightMin,
                        heightMax: heightMax);
                    //bottom
                    heightMap[midX, y] = CalculateMidPointHeight(
                        left: heightMap[x, y],
                        top: heightMap[midX, midY],
                        right: heightMap[cornerX, y],
                        bottom: heightMap[midX, pMidYD],
                        heightMin: heightMin,
                        heightMax: heightMax);
                }
            }

            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    float CalculateMidPointHeight(float left, float top, float right, float bottom, float heightMin, float heightMax)
    {
        return (left + top + right + bottom) / 4f + UnityEngine.Random.Range(heightMin, heightMax);
    }

    public void Voronoi()
    {
        var heightMap = GetHeightMap();

        for (int p = 0; p < voronoiPeaks; p++)
        {

            Vector3 peak = new Vector3(
                 UnityEngine.Random.Range(0, terrainData.heightmapResolution),
                 UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight),
                 UnityEngine.Random.Range(0, terrainData.heightmapResolution)
                 );


            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue; //prevent low peaks creating depressions inside mountains, if peek is below current height dont apply peaj to heightmap

            //Calculate slopes
            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0),
                new Vector2(terrainData.heightmapResolution,
                terrainData.heightmapResolution));

            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    if (!(x == peakLocation.x && y == peakLocation.y))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;

                        float h;

                        switch (voronoiType)
                        {
                            case VoronoiType.Combined:
                                h = peak.y - distanceToPeak * voronoiFallOff - Mathf.Pow(distanceToPeak, voronoiDropOff);//combined
                                break;
                            case VoronoiType.Power:
                                h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff;//power
                                break;
                            case VoronoiType.SinPow:
                                h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) -
                                    Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff;//sinpow
                                break;
                            case VoronoiType.Linear:
                            default:
                                h = peak.y - distanceToPeak * voronoiFallOff;//linear
                                break;
                        }

                        //new peak data will not cancel out previous height data
                        if (heightMap[x, y] < h)
                            heightMap[x, y] = h;
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                //Old simple perlin
                //heightMap[x, y] = Mathf.PerlinNoise(
                //    (x + perlinOffsetX) * perlinXScale,
                //    (y + perlinOfsetY) * perlinYScale);

                //Perlin with BrownianMotion
                heightMap[x, y] += Utils.fBM(
                    (x + perlinOffsetX) * perlinXScale,
                    (y + perlinOffsetY) * perlinYScale,
                    perlinOctaves,
                    perlinPersistance) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                foreach (var p in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM(
                       (x + p.mPerlinOffsetX) * p.mPerlinXScale,
                       (y + p.mPerlinOffsetY) * p.mPerlinYScale,
                       p.mPerlinOctaves,
                       p.mPerlinPersistance) * p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        var keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }

        if (keptPerlinParameters.Count == 0) //don't want to keep any
        {
            keptPerlinParameters.Add(perlinParameters[0]);//add at least 1
        }
        perlinParameters = keptPerlinParameters;
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }

    public void LoadTexture()
    {
        float[,] heightMap;
        heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                          (int)(z * heightMapScale.z)).grayscale
                                                            * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain()
    {
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                heightMap[x, z] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }

    void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    void Awake()
    {
        SerializedObject tagManager = new SerializedObject(
                              AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        //apply tag changes to tag database
        tagManager.ApplyModifiedProperties();

        //take this object
        this.gameObject.tag = "Terrain";
    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;
        //ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; break; }
        }
        //add your new tag
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
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
