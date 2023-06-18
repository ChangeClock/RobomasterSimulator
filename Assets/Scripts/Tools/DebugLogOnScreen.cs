using UnityEngine;
using System.Collections.Generic;
using System;

public class DebugLogOnScreen : MonoBehaviour
{
    public int fontSize = 2;
    private int logNo = 0;
    private string debugLog = "";
    private const int maxLines = 10;
    private Queue<string> debugLogQueue = new Queue<string>();
    
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (debugLogQueue.Count >= maxLines)
        {
            debugLogQueue.Dequeue();
        }
    
        debugLogQueue.Enqueue(logString);
        
        debugLog = "";
        foreach (string s in debugLogQueue)
        {
            debugLog += "[" + logNo + "] " + s + "\n";
            logNo += 1;
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = fontSize;

        GUI.Label(new Rect(10, Screen.height - 200, Screen.width, Screen.height), debugLog);
    }
}
