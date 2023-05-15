using UnityEngine;

public class LogTest : MonoBehaviour
{
    void Start()
    {
        int x = 5;
        float y = 10.2f;
        string name = "Genie";
        Debug.Log($"Hello {name}, x = {x}, y = {y}");
    }
}
