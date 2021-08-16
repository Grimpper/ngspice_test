using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiceVariable
{
    private string name;
    private List<float> values;
    
    public string Name => name;
    public List<float> Values => values;

    public SpiceVariable(string name, List<float> values)
    {
        this.name = name;
        this.values = values;
    }
}
