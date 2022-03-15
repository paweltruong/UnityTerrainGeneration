using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float fBM(float x, float y, int octaves, float persistance)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }

        return total / maxValue;
    }

    /// <summary>
    /// https://www.arduino.cc/reference/en/language/functions/math/map/
    /// </summary>
    /// <param name="value"></param>
    /// <param name="originalMin"></param>
    /// <param name="originalMax"></param>
    /// <param name="targetMin"></param>
    /// <param name="targetMax"></param>
    /// <returns></returns>
    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }
}