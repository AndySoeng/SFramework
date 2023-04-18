using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace SFramework
{
public class SUpdateUI : MonoBehaviour
{
    [Title("ProgressUI")] [SerializeField] private TMP_Text txt_Tip;
    [SerializeField] private Image img_ProgressFill;
    [SerializeField] private TMP_Text txt_Progress;

    [Title("ModalWindowUI")] [SerializeField]
    private GameObject go_ModalWindow;

    [SerializeField] private TMP_Text txt_MWTitle;
    [SerializeField] private TMP_Text txt_MWContent;
    [SerializeField] private Button btn_MWConfirm;
    [SerializeField] private Button btn_MWCancel;

    private List<string> list_Tips = new List<string>()
    {
        "supdate0",
        "supdate1",
        "supdate2",
        "supdate3",
        "supdate4",
        "supdate5",
    };
    
    public void SetProgress(float progress, long? needDownloadSize = null, long? downloadedSize = null)
    {
        if (needDownloadSize == null || downloadedSize == null)
            UpdateTip(progress);
        else
            UpdateTip(progress, $"{downloadedSize / 1024f / 1024f:N2}MB/{needDownloadSize / 1024f / 1024f:N2}MB");
        img_ProgressFill.fillAmount = progress;
        txt_Progress.text = string.Format($"{(int)(progress * 100)}%");
    }

    public void ResetProgress()
    {
        txt_Tip.text = I2.Loc.LocalizationManager.GetTranslation(list_Tips[0]);
        img_ProgressFill.fillAmount = 0;
        txt_Progress.text = "0%";
    }

    private void UpdateTip(float progress, string jointStr = null)
    {
        int tipIndex = (int)(progress * (list_Tips.Count - 1));
        txt_Tip.text = I2.Loc.LocalizationManager.GetTranslation(list_Tips[tipIndex])+ (String.IsNullOrEmpty(jointStr) ? String.Empty : ("  " + jointStr));
    }

    public void ShowModalWindow(string title, string content, Action confirm = null, Action cancel = null)
    {
        txt_MWTitle.text = title;
        txt_MWContent.text = content;
        btn_MWConfirm.onClick.RemoveAllListeners();
        btn_MWCancel.onClick.RemoveAllListeners();
        btn_MWConfirm.onClick.AddListener(() =>
        {
            confirm?.Invoke();
            HideModalWindow();
        });

        btn_MWCancel.onClick.AddListener(() =>
        {
            cancel?.Invoke();
            HideModalWindow();
        });

        go_ModalWindow.SetActive(true);
    }

    public void HideModalWindow()
    {
        go_ModalWindow.SetActive(false);
    }
}}