using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    int textureResolution = 513;
    string filename = "myProceduralTexture";
    float perlinXScale = 0.001f;
    float perlinYScale = 0.001f;
    int perlinOctaves = 5;
    float perlinPersistance = 1.5f;
    float perlinHeightScale = 0.9f;
    int perlinOffsetX = 1000;
    int perlinOffsetY = 200;
    bool falloffToggle = false;
    float falloffStart = 0;
    float falloffEnd = 0;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool mapToggle = false;//remap color values, to tweak grayscale

    float brightness = 0.5f;
    float contrast = 0.5f;

    Texture2D pTexture;

    [MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        pTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, false);
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        filename = EditorGUILayout.TextField("Texture Name", filename);

        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        textureResolution = EditorGUILayout.IntSlider(textureResolution, 513, 100001);
        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0, 0.1f);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance, 1, 10);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0, 1);
        perlinOffsetX = EditorGUILayout.IntSlider("Offset X", perlinOffsetX, 0, 10000);
        perlinOffsetY = EditorGUILayout.IntSlider("Offset Y", perlinOffsetY, 0, 10000);

        brightness = EditorGUILayout.Slider("Brightness", brightness, 0, 2);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0, 2);

        falloffToggle = EditorGUILayout.Toggle("Falloff", falloffToggle);
        falloffStart = EditorGUILayout.Slider("FalloffStart", falloffStart, 0, 1);
        falloffEnd = EditorGUILayout.Slider("FalloffEnd", falloffEnd, 0, 1);

        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        mapToggle = EditorGUILayout.Toggle("Map?", mapToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless", seamlessToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        float minColor = 1;
        float maxColor = 0;

        if (GUILayout.Button("Generate", GUILayout.Width(wSize)))
        {
            int w = textureResolution;
            int h = textureResolution;
            float pValue;
            Color pixCol = Color.white;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (seamlessToggle)
                    {
                        float u = (float)x / (float)w;
                        float v = (float)y / (float)h;

                        //bottom left tile
                        float noise00 = Utils.fBM(
                            (x + perlinOffsetX) * perlinXScale,
                            (y + perlinOffsetY) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;
                        //top left tile
                        float noise01 = Utils.fBM(
                            (x + perlinOffsetX) * perlinXScale,
                            (y + perlinOffsetY + h) * perlinYScale,
                            perlinOctaves,
                              perlinPersistance) * perlinHeightScale;
                        //bottom right tile
                        float noise10 = Utils.fBM(
                            (x + perlinOffsetX + w) * perlinXScale,
                            (y + perlinOffsetY) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;
                        //top right tile
                        float noise11 = Utils.fBM(
                            (x + perlinOffsetX + w) * perlinXScale,
                            (y + perlinOffsetY + h) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;

                        float noiseTotal =
                            u * v * noise00 +
                            u * (1 - v) * noise01 +
                            (1 - u) * v * noise10 +
                            (1 - u) * (1 - v) * noise11;

                        float tileHalfResolution = (textureResolution - 1) / 2;
                        float offset = 50;//can be random
                        float value = (int)(tileHalfResolution * noiseTotal) + offset;
                        float r = Mathf.Clamp((int)noise00, 0, 255);
                        float g = Mathf.Clamp((int)value, 0, 255);
                        float b = Mathf.Clamp((int)value + offset, 0, 255);
                        float a = Mathf.Clamp((int)noise00 + offset * 2, 0, 255);

                        pValue = (r + g + b) / (3 * 255.0f);//greyscale
                    }
                    else
                    {
                        pValue = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;
                    }


                    float colValue = contrast * (pValue - 0.5f) + 0.5f * brightness;
                    if (minColor > colValue) minColor = colValue;
                    if (maxColor < colValue) maxColor = colValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture.SetPixel(x, y, pixCol);
                }
            }

            if (mapToggle)
            {
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        pixCol = pTexture.GetPixel(x, y);
                        float colValue = pixCol.r;//could be any r,g or b because we have greyscale
                        colValue = Utils.Map(colValue, minColor, maxColor, 0, 1);
                        pixCol.r = colValue;
                        pixCol.g = colValue;
                        pixCol.b = colValue;
                        pTexture.SetPixel(x, y, pixCol);
                    }
                }
            }

            if (falloffToggle)
            {
                ApplySimpleFallOff(pTexture, new Vector2Int(textureResolution, textureResolution), falloffStart, falloffEnd);
            }

            pTexture.Apply(false, false);
        }

        if (GUILayout.Button("Generate2", GUILayout.Width(wSize)))
        {
            int w = textureResolution;
            int h = textureResolution;
            float pValue;
            Color pixCol = Color.white;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {

                    pValue = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale,
                            perlinOctaves,
                            perlinPersistance) * perlinHeightScale;

                    float colValue = pValue;

                    //float colValue = contrast * (pValue - 0.5f) + 0.5f * brightness;
                    if (minColor > colValue) minColor = colValue;
                    if (maxColor < colValue) maxColor = colValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture.SetPixel(x, y, pixCol);
                }
            }

            if (mapToggle)
            {
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        pixCol = pTexture.GetPixel(x, y);
                        float colValue = pixCol.r;//could be any r,g or b because we have greyscale
                        colValue = Utils.Map(colValue, minColor, maxColor, 0, 1);
                        pixCol.r = colValue;
                        pixCol.g = colValue;
                        pixCol.b = colValue;
                        pTexture.SetPixel(x, y, pixCol);
                    }
                }
            }

            if (falloffToggle)
            {
                ApplySimpleFallOff(pTexture, new Vector2Int(textureResolution, textureResolution), falloffStart, falloffEnd);
            }

            pTexture.Apply(false, false);
        }


        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(pTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(wSize)))
        {
            byte[] bytes = pTexture.EncodeToPNG();
            System.IO.Directory.CreateDirectory(Application.dataPath + "/SavedTextures");
            var fullpath = Application.dataPath + "/SavedTextures/" + filename + ".png";
            File.WriteAllBytes(fullpath, bytes);
            Debug.Log($"Saved to: {fullpath}");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pTexture"></param>
    /// <param name="size"></param>
    /// <param name="falloffStart">in texture coords</param>
    /// <param name="falloffEnd">in texture coords</param>
    void ApplySimpleFallOff(Texture2D pTexture, Vector2Int size, float falloffStart, float falloffEnd)
    {

        for (int y = 0; y < size.y; ++y)
        {
            for (int x = 0; x < size.x; ++x)
            {
                Vector2 position = new Vector2(
                    (float)x / size.x * 2 - 1,
                    (float)y / size.x * 2 - 1
                    );

                //Find which value is closer to the edge
                float t = Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.y));
                var originalValue = pTexture.GetPixel(x, y);
                float falloffHeightMapValue = 0;
                if (t < falloffStart)
                {
                    falloffHeightMapValue = 1;
                    //pTexture.SetPixel(x, y, originalValue);//heightMap[x, y] = 1;
                    //pTexture.SetPixel(x, y, originalValue);//heightMap[x, y] = 1;
                }
                else if (t > falloffEnd)
                {
                    falloffHeightMapValue = 0;
                    //pTexture.SetPixel(x, y, FloatToGrayscale(0));//heightMap[x, y] = 0;
                }
                else
                {
                    //pTexture.SetPixel(x, y, Color.black);
                    //var lerpValue = Mathf.InverseLerp(falloffStart, falloffEnd, t);
                    //var step = Mathf.SmoothStep(0, 1, lerpValue);
                    //pTexture.SetPixel(x, y, FloatToGrayscale(GrayscaleToFloat(originalValue) * step));
                    //if (x % 4 == 0 && y % 4 == 0)
                    //    Debug.Log($"X,Y=({x},{y}) T:{t} Lerp:{lerpValue} Step:{step} Col:{FloatToGrayscale(GrayscaleToFloat(originalValue))} FinalC:{FloatToGrayscale(GrayscaleToFloat(originalValue) * step)}");
                    //pTexture.SetPixel(x, y, FloatToGrayscale(
                    //    Mathf.SmoothStep(0, GrayscaleToFloat(originalValue), lerpValue)
                    //    ));
                    //heightMap[x, y] = Mathf.SmoothStep(1, 0, Mathf.InverseLerp(falloffStart, falloffEnd, t));
                    falloffHeightMapValue = Mathf.SmoothStep(1, 0, Mathf.InverseLerp(falloffStart, falloffEnd, t));
                }
                pTexture.SetPixel(x, y, originalValue * falloffHeightMapValue);
            }
        }

        pTexture.Apply(false, false);
    }

    Color FloatToGrayscale(float colValue, float minColor = 0,
    float maxColor = 1, bool alphaToggle = false)
    {
        var mappedValue = Utils.Map(colValue, minColor, maxColor, 0, 1);
        var color = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
        color.r = mappedValue;
        color.g = mappedValue;
        color.b = mappedValue;
        return color;
    }


    float GrayscaleToFloat(Color color, float minColor = 1,
    float maxColor = 0, bool alphaToggle = false)
    {
        if (color.r != color.g && color.g != color.b)
        {
            Debug.LogError("Color is not grayscale");
        }

        //var mappedValue = Utils.Map(color.r, minColor, maxColor, 0, 1);
        return color.r;
    }
}
