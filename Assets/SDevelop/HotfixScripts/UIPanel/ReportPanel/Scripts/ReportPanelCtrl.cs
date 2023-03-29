namespace SFramework.UI
{
    using System.Collections.Generic;
    using Michsky.UI.ModernUIPack;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using SFramework;

    public class ReportPanelCtrl : UICtrlBase
    {
        [Title("实验报告分页")] public CanvasGroup panel;
        public TMP_Text txt_Date;
        public TMP_Text txt_UseTime;
        public CustomDropdown dp_Conclusion;
        public TMP_InputField input_Conclusion;
        public HorizontalSelector StarRating;
        public TMP_Text txt_TotalScore;
        public Button btn_ReportCommit;
        public Transform ReportList;

        public Button btn_Close;

        private void Update()
        {
            txt_UseTime.text = Ex.ExTime.GetTime(Time.realtimeSinceStartup);
        }
        
    }
}