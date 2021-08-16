using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class SpiceVariable
{
    public List<float> values;
    
    public string Name { get; }

    public SpiceVariable(string name, List<float> values)
    {
        Name = name;
        this.values = values;
    }

    public List<float> GetValues(char magnitude = ' ')
    {
        List<float> scaledList = new List<float>(values);
        for (int i = 0; i < values.Count; i++)
        {
            scaledList[i] /= NumberUtils.GetMagnitude(magnitude); 
        }

        return scaledList;
    }
}
