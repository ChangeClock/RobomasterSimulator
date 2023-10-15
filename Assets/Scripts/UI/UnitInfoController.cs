using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitInfoController : MonoBehaviour 
{
    [SerializeField] CanvasGroup Info;
    [SerializeField] BarController HPBar;
    [SerializeField] TMP_Text ID;
    [SerializeField] GameObject[] LevelIcons;

    [SerializeField] TMP_Text ReviveTime;

    public void SetID(int id)
    {
        ID.text = id.ToString();
    }

    public void SetHP(float hp, float hplimit)
    {
        HPBar.SetMaxValue(hplimit);
        HPBar.SetValue(hp);
    }

    public void SetLevel(int level)
    {
        for (int i = 0; i < LevelIcons.Length; i++)
        {
            if (level == i)
            {
                LevelIcons[i].SetActive(true);
            } else {
                LevelIcons[i].SetActive(false);
            }
        }
    }

    public void SetReviveTime(bool enable, float time)
    {
        ReviveTime.gameObject.SetActive(enable);

        if (enable)
        {
            Info.alpha = 0.6f;
            ReviveTime.text = time.ToString("N0") + "s";
        } else {
            Info.alpha = 1f;
        }
    }
}