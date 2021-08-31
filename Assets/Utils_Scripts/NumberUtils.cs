using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberUtils
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

    private static string MagnitudeToString(this Unit magnitude) 
        => magnitude == Unit.Unitary ? "" : magnitude.ToString();

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
            while (absNumber < 1)
            {
                absNumber *= 10f;
                decimalPlaces--;
            }
        }
        else
        {
            while (absNumber > 10)
            {
                absNumber /= 10f;
                decimalPlaces++;
            }
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
    
    public static string GetUnit(string variableName, Unit magnitude)
    {
        if (variableName.Equals("time"))
        {
            return " (" + magnitude.MagnitudeToString() + Time + ")";
        }

        if (variableName.StartsWith("v"))
        {
            return " (" + magnitude.MagnitudeToString() + Voltage + ")";
        }

        if (variableName.StartsWith("i"))
        {
            return " (" + magnitude.MagnitudeToString() + Intensity + ")";
        }

        return "";
    }
}
