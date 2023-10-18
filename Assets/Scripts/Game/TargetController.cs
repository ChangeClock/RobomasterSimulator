using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour 
{
    public int TargetID;
    public bool IsActive = false;
    public bool IsTarget = false;

    public int Score = 0;

    public List<GameObject> Rings = new List<GameObject>();
    public GameObject TargetIcon;

    public delegate void ScoreAction(int id, int score);
    public event ScoreAction OnScore;

    void Update()
    {
        if (IsActive && Score > 0)
        {
            for (int i = 0; i < Rings.Count; i ++)
            {
                Rings[i].SetActive(Score == i);
            }
            TargetIcon.SetActive(false);
        } else {           
            foreach (var ring in Rings)
            {
                if (ring != null) ring.SetActive(false);
            }
            TargetIcon.SetActive(IsTarget);
        }
    }

    public void Reset()
    {
        IsActive = false;
        IsTarget = false;
        Score = 0;
    }
}