using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class SpiceParser : MonoBehaviour
{
    static string path = System.IO.Directory.GetCurrentDirectory() + "/Spice64/circuits/test_circuit_output.txt";
    
    public static void WriteString(string str)
    {
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(str);
        writer.Close();
    }

    public static void ReadString()
    {
        Debug.Log("Reading from: " + path);
        
        Dictionary<string, SpiceVariable> variables = new Dictionary<string, SpiceVariable>();
        
        StreamReader file = new StreamReader(path);
        string line;
        
        int numberOfVariables = 0;

        while ((line = file.ReadLine()) != null)
        {
            Debug.Log("Reading...");
            if (line.Contains("No. Variables:"))
            {
                Regex regexNumVar = new Regex(@"No. Variables: (\d+)");
                Match varMatch = regexNumVar.Match(line);
                numberOfVariables = int.Parse(varMatch.Groups[1].Value);
            }
            
            if (line.Equals("Variables:"))
            {
                Debug.Log("Variables found");
                
                Regex regexVariables = new Regex(@"\t(\d)\t(.+)\t(.+)");
                string varLine;
                
                while ((varLine = file.ReadLine()) != null && !varLine.Contains("Values:"))
                {
                    Match varMatch = regexVariables.Match(varLine);
                    SpiceVariable variable = new SpiceVariable(varMatch.Groups[2].Value, new List<double>());
                    
                    Debug.Log("Line: " + varLine);
                    
                    variables.Add(varMatch.Groups[1].Value, variable);
                    Debug.Log("groups: " + varMatch.Groups[1].Value + " -- " + variable.Name);
                }
                
                Debug.Log("Values found");
                
                Regex regexValues = new Regex(@"( \d)?\t(.+)");
                
                if (!line.Equals("Values:"))
                {
                    Debug.Log("Values found");
                    continue;
                }
                
                while ((varLine = file.ReadLine()) != null)
                {
                    for (int i = 0; i < numberOfVariables | (varLine = file.ReadLine()) != null; i++)
                    {
                        variables.TryGetValue(i.ToString(), out SpiceVariable variable);
                        
                        Match varMatch = regexVariables.Match(varLine);
                        
                        variable.Values.Add(double.Parse(varMatch.Groups[2].Value));
                    } 
                }
            }
        }

        file.Close();
    }
}
