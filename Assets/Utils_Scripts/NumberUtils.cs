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
}
