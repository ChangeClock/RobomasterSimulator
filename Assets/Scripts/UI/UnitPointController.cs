using System;
using UnityEngine;
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

    public void SetDirection(float angle)
    {
        Arrow.transform.Rotate(0f, 0f, angle);
    }
}