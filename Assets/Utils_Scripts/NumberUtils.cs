using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberUtils : MonoBehaviour
{
    public const string
        Radians = "rad",
        Degrees = "°",
        Time = "s",
        Voltage = "V",
        Intensity = "I";

    public static int GetSignificantFigurePos(float number)
    {
        float absNumber = Math.Abs(number);

        // TODO fix sigFigPos for numbers beyond 1
        if (absNumber > 1 || absNumber == 0) return 0;
        
        int decimalPlaces = 0;
        while (Math.Floor(absNumber) == 0)
        {
            absNumber *= 10;
            decimalPlaces++;
        }

        return decimalPlaces;
    }

    public static float? GetAngleFromVector(Vector2 vector, string unit = Radians) =>
        unit switch
        {
            Degrees => (float)(Math.Atan(vector.y / vector.x) * 180 / Math.PI),
            Radians => (float)Math.Atan(vector.y / vector.x),
            _ => null
        };

    public static float GetMagnitude(char magnitude) =>
        magnitude switch
        {
            'Y' => Mathf.Pow(10, 24),
            'Z' => Mathf.Pow(10, 21),
            'E' => Mathf.Pow(10, 18),
            'P' => Mathf.Pow(10, 15),
            'T' => Mathf.Pow(10, 12),
            'G' => Mathf.Pow(10, 9),
            'M' => Mathf.Pow(10, 6),
            'k' => Mathf.Pow(10, 3),
            'h' => Mathf.Pow(10, 2),
            'D' => Mathf.Pow(10, 1),
            ' ' => Mathf.Pow(10, 0),
            'd' => Mathf.Pow(10, -1),
            'c' => Mathf.Pow(10, -2),
            'm' => Mathf.Pow(10, -3),
            'u' => Mathf.Pow(10, -6),
            'n' => Mathf.Pow(10, -9),
            'p' => Mathf.Pow(10, -12),
            'f' => Mathf.Pow(10, -15),
            'a' => Mathf.Pow(10, -18),
            'z' => Mathf.Pow(10, -21),
            'y' => Mathf.Pow(10, -24),
            _ => throw new ArgumentOutOfRangeException(nameof(magnitude), magnitude, null)
        };
}
