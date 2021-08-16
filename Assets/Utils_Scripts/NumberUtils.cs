using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberUtils : MonoBehaviour
{
    public enum Unit
    {
        Y = 24,
        Z = 21,
        E = 18,
        P = 15,
        T = 12,
        G = 9,
        M = 6,
        k = 3,
        h = 2,
        D = 1,
        Unitary = 0,
        d = -1,
        c = -2,
        m = -3,
        u = -6,
        n = -9,
        p = -12,
        f = -15,
        a = -18,
        z = -21,
        y = -24,
    }
    
    public const string
        Radians = "rad",
        Degrees = "°",
        Time = "s",
        Voltage = "V",
        Intensity = "I";

    public static int GetSignificantFigurePos(float number)
    {
        float absNumber = Math.Abs(number);

        if (absNumber == 0) 
            return 0;
        
        int decimalPlaces = 0;

        if (absNumber < 1)
        {
            while ((absNumber *= 10f) < 1)
                decimalPlaces--;
        }
        else
        {
            while ((absNumber /= 10f) > 1)
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

    public static float GetMagnitude(Unit unit) => Mathf.Pow(10, (int) unit);
}
