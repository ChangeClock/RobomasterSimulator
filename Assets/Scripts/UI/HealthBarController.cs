using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour 
{
    [SerializeField] private GameObject divisionLine;
    [SerializeField] private bool displayDivision = false;
    [SerializeField] private float divisionStep = 0.5f;
    // [SerializeField] private GameObject[] lines;

    [SerializeField] private Slider HealthBar;
    [SerializeField] private Image Boarder;

    [SerializeField] private RectTransform recTransform;
    private Vector2 Size;

    void Start()
    {
        if (recTransform != null)
        {
            Size = recTransform.sizeDelta;
        }

        // UpdateDivision();
    }

    void OnEnable()
    {
        UpdateDivision();
    }

    void Update()
    {
        
    }

    public void SetMaxHealth(float max)
    {
        if (HealthBar != null) HealthBar.maxValue = max;
        UpdateDivision();
    }

    public void SetHealth(float var)
    {
        if (HealthBar != null) HealthBar.value = var;
    }

    public void SetBoardColor(Color color)
    {
        if (Boarder != null) Boarder.color = color;
    }

    public void SetDisplayDivision(bool var)
    {
        displayDivision = var;
        UpdateDivision();
    }

    public void SetDivisionStep(float step)
    {
        divisionStep = step;
        UpdateDivision();
    }

    public void UpdateDivision()
    {
        GameObject[] _lines = GameObject.FindGameObjectsWithTag("DivLines");

        foreach (GameObject var in _lines)
        {
            Destroy(var);
        }

        if (displayDivision)
        {
            float pixelsPerDiv = Size.y * (divisionStep / HealthBar.maxValue);
            int lineCounts = (int)Math.Round(HealthBar.maxValue / divisionStep - 1);
            float start = - Size.y / 2;

            Debug.Log($"Health Bar have {lineCounts} lines");

            GameObject[] newLines = new GameObject[lineCounts];

            for(int i = 0; i < lineCounts ; i++)
            {
                Debug.Log($"Drawing Line {i + 1}");

                // TODO: Instantiate divisionline according to line counts
                // newLines[i] = Instantiate(divisionLine, Vector3.right * ((i+1) * pixelsPerDiv + start), Quaternion.identity);
                // newLines[i].SetActive(true);
            }

            // _line = Instantiate(divisionLine, Vector3.right * (pixelsPerDiv + start), Quaternion.identity);

            // Destroy(_line);
        } else {

        }
    }
}