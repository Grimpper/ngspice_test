using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Ngspice : MonoBehaviour
{
    [SerializeField] private bool debug = false;
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Launch ngspice"))
        {
            Process ngspiceProcess = SpiceManager.GetProcess();

            ngspiceProcess.Start();
            LogRunningProcessName("ngspice_con");
            
            //StreamWriter inputStream = SpiceManager.GetInputStream();
//
            //inputStream.WriteLine("help all");
            //inputStream.Flush();
//
            if (debug) Debug.Log(ngspiceProcess.StandardOutput.ReadToEnd());
            //LogRunningProcessName("ngspice_con");
            
            ngspiceProcess.WaitForExit();
        }
        //
        // if (GUI.Button(new Rect(170, 10, 150, 50), "Compute output") && inputStream != null)
        // {
        //     LogRunningProcessName("ngspice_con");
        //     //inputStream.WriteLine("op");
        //     //inputStream.Flush();
        // }
    }

    private void LogRunningProcessName(string strName)
    {
        Process[] processes = Process.GetProcessesByName(strName);
        
        if (processes.Length > 0)
        {
            foreach (var process in processes)
                Debug.Log(process.ProcessName+ " is running");
        }
    }
}
