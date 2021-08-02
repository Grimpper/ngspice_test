using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiceVariable
{
    private string name;
    private List<double> values;

    public SpiceVariable(string name, List<double> values)
    {
        this.name = name;
        this.values = values;
    }

    public string Name => name;
    public List<double> Values => values;
}
