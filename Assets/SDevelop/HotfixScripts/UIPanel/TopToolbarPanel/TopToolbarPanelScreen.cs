namespace SFramework.UI
{
    using SFramework;

    public class TopToolbarPanelScreenParam : UIOpenScreenParameterBase
    {
    }

    public class TopToolbarPanelScreen : UIScreenBase
    {
        TopToolbarPanelCtrl mCtrl;
        TopToolbarPanelScreenParam mParam;

        protected override void  OnLoadSuccess()
        {
            mCtrl = mCtrlBase as TopToolbarPanelCtrl;
            mParam = mOpenParam as TopToolbarPanelScreenParam;

            mCtrl.btn_Back.onClick.AddListener(() =>
            {
                SUIManager.Ins.CloseAllUI();
                LoadScenePanelScreen.LoadSingleScene(LoadSceneName.Scene_Main, () =>
                {
                    
                });
            });
            
        }

       
    }
}