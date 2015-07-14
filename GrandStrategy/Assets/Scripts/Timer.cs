using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class Timer 
{

    public static int timer;
    public static string taskName;

    public static List<String> taskLogs = new List<string>();


    public static void Start(string name) 
    {
        taskName = name;
        timer = System.Environment.TickCount; 
    }

    public static void End()
    {
        taskLogs.Add(taskName + ": " + ((System.Environment.TickCount - timer) * 0.001f) + " seconds");
    }

    public static void Print() 
    {
        for (int i = 0; i < taskLogs.Count; i++)
        {
            Debug.Log(taskLogs[i]);
        }
        taskLogs.Clear();
    }

}
