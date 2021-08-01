using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class SpiceParser : MonoBehaviour
{
    static readonly string Path = Directory.GetCurrentDirectory() + "/Spice64/circuits/test_circuit_output.txt";
    
    public static void WriteString(string str)
    {
        StreamWriter writer = new StreamWriter(Path, true);
        writer.WriteLine(str);
        writer.Close();
    }

    public static void ReadString()
    {
        Debug.Log("Reading from: " + Path);
        
        Dictionary<string, SpiceVariable> variables = new Dictionary<string, SpiceVariable>();
        
        StreamReader file = new StreamReader(Path);

        ParseVariables(file, ref variables, VariableCount(file));
        ParseValues(file, ref variables);

        LogSpiceVariables(in variables);
            
        file.Close();
    }

    private static int VariableCount(in StreamReader file)
    {
        string line;

        while ((line = file.ReadLine()) != null)
        {
            if (!line.Contains("No. Variables:")) continue;
            
            Regex regexNumVar = new Regex(@"No. Variables: (\d+)");
            Match varMatch = regexNumVar.Match(line);
            
            return int.Parse(varMatch.Groups[1].Value);
        }

        return -1;
    }

    private static void ParseVariables(in StreamReader file, ref Dictionary<string, SpiceVariable> variables,
        int numberOfVariables)
    {
        string line;

        while ((line = file.ReadLine()) != null)
        {
            if (!line.Equals("Variables:")) continue;
            
            //Debug.Log("Variables found");

            Regex regexVariables = new Regex(@"\t(\d)\t(.+)\t(.+)");

            for (int i = 0; i < numberOfVariables; i++)
            {
                if ((line = file.ReadLine()) == null) break;
                
                Match varMatch = regexVariables.Match(line);
                SpiceVariable variable = new SpiceVariable(varMatch.Groups[2].Value, new List<double>());

                //Debug.Log("Line: " + line);

                variables.Add(varMatch.Groups[1].Value, variable);
                //Debug.Log("groups: " + varMatch.Groups[1].Value + " -- " + variable.Name);
            }
            
            break;
        }
    }
    private static void ParseValues(in StreamReader file, ref Dictionary<string, SpiceVariable> variables)
    {
        string line;
        bool valuesFound = false;

        while ((line = file.ReadLine()) != null)
        {
            if (!valuesFound && !line.Equals("Values:")) continue;

            valuesFound = true;
            //Debug.Log("Values found");

            Regex regexValues = new Regex(@"( \d)?\t(.+)");
            
            for (int i = 0; i < variables.Count; i++)
            {
                if ((line = file.ReadLine()) == null) break;
                
                variables.TryGetValue(i.ToString(), out SpiceVariable variable);
                
                Match varMatch = regexValues.Match(line);
                
                //Debug.Log("Value: " + varMatch.Groups[2].Value);
                
                variable?.Values.Add(double.Parse(varMatch.Groups[2].Value));
            }
        }
    }

    private static void LogSpiceVariables(in Dictionary<string, SpiceVariable> variables)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            variables.TryGetValue(i.ToString(), out SpiceVariable variable);
            
            if (variable == null) return;
            
            Debug.Log(variable.Name + ": \n");

            foreach (double value in variable.Values)
            {
                Debug.Log(value + ", ");
            }
        }
    }
    
}
