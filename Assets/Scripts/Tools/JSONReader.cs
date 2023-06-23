using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    public static string LoadResourceTextfile(string path)
    {
        string filePath = "Config/" + path.Replace(".json", "");

        TextAsset targetFile = Resources.Load<TextAsset>(filePath);

        return targetFile.text;
    }
}
