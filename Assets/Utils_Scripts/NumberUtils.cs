using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberUtils : MonoBehaviour
{
    public enum Unit
    {
        radians,
        degrees
    }
    
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

    public static float GetAngleFromVector(Vector2 vector, Unit unit = Unit.radians)
    {
        if (unit == Unit.degrees)
        {
            return (float) (Math.Atan(vector.y / vector.x) * 180 / Math.PI);
        }
        
        return (float) Math.Atan(vector.y / vector.x);
    }
}
