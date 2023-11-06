using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitPointController : MonoBehaviour 
{
    [SerializeField] private GameObject Boarder;
    [SerializeField] private GameObject Arrow;
    [SerializeField] private TextMeshProUGUI ID;
    [SerializeField] private GameObject Sentry;

    public void IsHighligh(bool var)
    {
        Boarder.SetActive(var);
    }

    public void SetID(int id)
    {
        if (id == 7)
        {
            Sentry.SetActive(true);
            ID.text = "";
        } else {
            Sentry.SetActive(false);
            ID.text = id.ToString();
        }
    }

    public void SetColor(Color color)
    {
        gameObject.GetComponent<RawImage>().color = color;
        Arrow.GetComponent<RawImage>().color = color;
    }

    public void SetDirection(float angle)
    {
        if (!Arrow.activeSelf) Arrow.SetActive(true);

        if (Mathf.Abs(Arrow.transform.eulerAngles.z - angle) <= 0.1f) return;

        // Debug.Log($"[UnitPoint] angle delta {Arrow.transform.eulerAngles.z - angle}");

        Arrow.transform.Rotate(0f, 0f, angle - Arrow.transform.eulerAngles.z);
    }
}