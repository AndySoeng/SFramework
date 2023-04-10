using Cysharp.Threading.Tasks;

namespace SFramework.UI
{
    using SFramework;
    using Michsky.UI.ModernUIPack;
    using UnityEngine.Events;

    public class ModalWindowScreenParam : UIOpenScreenParameterBase
    {
    }

    public class ModalWindowPanelScreen : UIScreenBase
    {
        ModalWindowPanelCtrl mCtrl;
        ModalWindowScreenParam mParam;

        protected override void OnLoadSuccess()
        {
            mCtrl = mCtrlBase as ModalWindowPanelCtrl;
            mParam = mOpenParam as ModalWindowScreenParam;
        }

        /// <summary>
        /// 打开模态窗口
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="confirm"></param>
        /// <param name="cancel"></param>
        /// <param name="style"></param>
        public static async UniTask<ModalWindowPanelScreen> OpenModalWindowNoTabs(string title, string description, bool showConfirmButton = true, UnityAction confirm = null,
            bool showCancelButton = true,
            UnityAction cancel = null,
            ModalWindowStyle style = ModalWindowStyle.Style2,
            bool needCountdown = false,
            int countdownLength = 3)
        {
            UIScreenBase sb = await SUIManager.Ins.OpenUI<ModalWindowPanelScreen>();

            ModalWindowPanelScreen modalWindowPanelScreen = sb as ModalWindowPanelScreen;

            ModalWindowManager modalWindowManager = modalWindowPanelScreen.mCtrl.ModalWindowManagerDic[style + "" + ModalWindowType.Standard];
            //参数设置
            modalWindowManager.titleText = title;
            modalWindowManager.descriptionText = description;
            modalWindowManager.UpdateUI();
            modalWindowManager.onConfirm.RemoveAllListeners();
            modalWindowManager.onCancel.RemoveAllListeners();


            modalWindowManager.confirmButton.gameObject.SetActive(false);
            if (showConfirmButton)
            {
                if (needCountdown)
                {
                    modalWindowPanelScreen.mCtrl.StartCoroutine(modalWindowPanelScreen.mCtrl.CountdownActive(modalWindowManager.confirmButton.gameObject, countdownLength));
                }
                else
                {
                    modalWindowManager.confirmButton.gameObject.SetActive(true);
                }
            }


            if (confirm != null)
                modalWindowManager.onConfirm.AddListener(confirm);


            modalWindowManager.cancelButton.gameObject.SetActive(false);
            if (showCancelButton)
            {
                if (needCountdown)
                {
                    modalWindowPanelScreen.mCtrl.StartCoroutine(modalWindowPanelScreen.mCtrl.CountdownActive(modalWindowManager.cancelButton.gameObject, countdownLength));
                }
                else
                {
                    modalWindowManager.cancelButton.gameObject.SetActive(true);
                }
            }

            if (cancel != null)
                modalWindowManager.onCancel.AddListener(cancel);
            //-------
            modalWindowManager.OpenWindow();

            return modalWindowPanelScreen;
        }

        /// <summary>
        /// 未完善
        /// </summary>
        /// <param name="style"></param>
        private static async UniTask<ModalWindowPanelScreen> OpenModalWindowWithTabs(ModalWindowStyle style = ModalWindowStyle.Style2)
        {
            UIScreenBase sb = await SUIManager.Ins.OpenUI<ModalWindowPanelScreen>();
            ModalWindowPanelScreen modalWindowPanelScreen = sb as ModalWindowPanelScreen;
            ModalWindowManager modalWindowManager = modalWindowPanelScreen.mCtrl.ModalWindowManagerDic[style + "" + ModalWindowType.WithTabs];
            //参数设置
            //-------
            modalWindowManager.OpenWindow();
            return modalWindowPanelScreen;
        }
    }

    /// <summary>
    /// 模态窗口风格
    /// </summary>
    public enum ModalWindowStyle
    {
        Style1,
        Style2,
    }

    /// <summary>
    /// 是否带Tab
    /// </summary>
    public enum ModalWindowType
    {
        Standard,
        WithTabs,
    }
}