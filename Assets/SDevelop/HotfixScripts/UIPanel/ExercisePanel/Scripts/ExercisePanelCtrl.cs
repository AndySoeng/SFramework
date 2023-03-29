namespace SFramework.UI
{
    using SFramework;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    public class ExercisePanelCtrl : UICtrlBase
    {
        [Title("实验习题分页")] public CanvasGroup panel;
        public Button btn_LabQuestRefresh;
        public Button btn_LabQuestCommit;
        public Button btn_LabQuestClose;
        public GameObject go_Title;
        public GameObject go_CloseTip;
        
        public Transform trans_LabQuestParent;
    }
}