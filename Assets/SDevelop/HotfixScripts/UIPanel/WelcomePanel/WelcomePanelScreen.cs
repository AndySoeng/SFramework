﻿

using Cysharp.Threading.Tasks;

namespace SFramework.UI
{
    using SFramework;

    public class WelcomePanelScreenParam : UIOpenScreenParameterBase
{
    public string mLoadSceneName;
}

    public class WelcomePanelScreen : UIScreenBase
    {
        WelcomePanelCtrl mCtrl;
        WelcomePanelScreenParam mParam;
        protected override async UniTask OnLoadSuccess()
        {
            await base.OnLoadSuccess();
            mCtrl = mCtrlBase as WelcomePanelCtrl;
            mParam = mOpenParam as WelcomePanelScreenParam;
            mCtrl.MainBtn.onClick.AddListener(OpenMainPanel);

        }

        protected void OpenMainPanel()
        {
            //GameUIManager.GetInstance().OpenUI<MainPanelScreen>();
            SUIManager.Ins.CloseUI<WelcomePanelScreen>();

        }

    }
}