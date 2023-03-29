namespace SFramework.UI
{
    using SFramework;

    public class HttpRequestPanelScreenParam : UIOpenScreenParameterBase
    {
        public string content;
    }

    public class HttpRequestPanelScreen : UIScreenBase
    {
        HttpRequestPanelCtrl mCtrl;
        HttpRequestPanelScreenParam mParam;

        protected override void OnLoadSuccess()
        {
            mCtrl = mCtrlBase as HttpRequestPanelCtrl;
            mParam = mOpenParam as HttpRequestPanelScreenParam;
            mCtrl.content.text = mParam.content;
        }
    }
}