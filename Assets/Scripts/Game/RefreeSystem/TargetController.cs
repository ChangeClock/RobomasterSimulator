using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetController : ArmorController 
{
    public int TargetID;
    public bool IsActive = false;
    public bool IsTarget = false;

    public int Score = 0;

    public List<GameObject> Rings = new List<GameObject>();
    public GameObject TargetIcon;
    public GameObject TargetRing;

    public GameObject TargetArm;
    public float targetArmOffset = 0.0f;
    public GameObject ActiveArm;

    public delegate void ScoreAction(int id, int score);
    public event ScoreAction OnScore;

    void Start()
    {

    }

    void Update()
    {
        if (!Enabled) return;

        if (IsActive && Score > 0)
        {
            if (Score == Rings.Count)
            {
                Rings[Score-1].SetActive(true);
                for (int i = Rings.Count - 2; i >= 0; i -= 2)
                {
                    Rings[i].SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < Rings.Count; i ++)
                {
                    Rings[i].SetActive(Score == i);
                }
            }
            if (TargetIcon != null) TargetIcon.SetActive(false);
            if (TargetRing != null) TargetRing.SetActive(true);
            if (TargetArm != null) TargetArm.SetActive(false);
            if (ActiveArm != null) ActiveArm.SetActive(true);
        } else {           
            foreach (var ring in Rings)
            {
                if (ring != null) ring.SetActive(false);
            }
            if (TargetIcon != null) TargetIcon.SetActive(IsTarget);
            if (TargetRing != null) TargetRing.SetActive(IsTarget);
            if (TargetArm != null) TargetArm.SetActive(IsTarget);
            if (ActiveArm != null) ActiveArm.SetActive(false);
        }

        Color color = Color.white;
        if (faction == Faction.Red) color = Color.red;
        if (faction == Faction.Blue) color = Color.blue;

        foreach (var ring in Rings)
        {
            if (ring != null) ring.GetComponent<RawImage>().color = color;
        }

        if (TargetArm != null)
        {
            TargetArm.GetComponent<RawImage>().color = color;

            targetArmOffset -= Time.deltaTime;
            if (targetArmOffset < -1.0f) targetArmOffset += 1.0f;

            Rect rect = TargetArm.GetComponent<RawImage>().uvRect;
            TargetArm.GetComponent<RawImage>().uvRect = new Rect(rect.x, targetArmOffset, rect.width, rect.height); 
        }

        if (TargetIcon != null) TargetIcon.GetComponent<RawImage>().color = color;
        if (TargetRing != null) TargetRing.GetComponent<RawImage>().color = color;
        if (ActiveArm != null) ActiveArm.GetComponent<RawImage>().color = color;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (ArmorCollider != null)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // Debug.DrawRay(contact.point, contact.normal, Color.white);
                // Debug.Log($"[ArmorController] Armor on Hit with {contact.thisCollider.name}");
                if (contact.thisCollider != ArmorCollider) return;
            }
        }

        int score = 0;
        int _score = 0;

        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawLine(contact.point, transform.position, Color.red);
            // Debug.Log($"[TargetController] Distance {Vector3.Distance(contact.point, transform.position)}, Radius {contact.thisCollider.bounds.size}, Score {Vector3.Distance(contact.point, contact.thisCollider.transform.position) / (contact.thisCollider.bounds.size.z / 2) * 10}");
            
            _score = 10 - Mathf.RoundToInt(Vector3.Distance(contact.point, contact.thisCollider.transform.position) / (contact.thisCollider.bounds.size.z / 2) * 10);

            if (_score > score) score = _score;
        }

        // Debug.Log($"[TargetController] Target {TargetID}, score {score}");
        if (OnScore != null) OnScore(TargetID, score);
    }

    public void Reset()
    {
        IsActive = false;
        IsTarget = false;
        Score = 0;
    }
}