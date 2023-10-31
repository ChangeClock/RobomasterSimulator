using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ExchangeLevelItemController : MonoBehaviour 
{
    public bool Checked = false;
    [SerializeField] private GameObject CheckedBackground;
    [SerializeField] private GameObject CheckMark;

    [SerializeField] private TMP_Text Level;
    [SerializeField] private TMP_Text Silver;
    [SerializeField] private TMP_Text Gold;

    void OnEnable()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => Toggle(true));
    }

    public void Toggle(bool enable)
    {
        Checked = enable;
        CheckedBackground.SetActive(Checked);
        CheckMark.SetActive(Checked);
    }

    public void SetContent(String level, String silverValue, String goldValue)
    {
        Level.text = level;
        Silver.text  = silverValue;
        Gold.text = goldValue;
    }

    public void SetAvailability(bool enable)
    {
        gameObject.GetComponent<Button>().interactable = enable;
        gameObject.GetComponent<CanvasGroup>().alpha = enable ? 1 : 0.4f;
    }
}