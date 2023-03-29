namespace SFramework.UI
{
    using SFramework;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;

    public class DemoPanelScreenParam : UIOpenScreenParameterBase
    {
    }

    public class DemoPanelScreen : UIScreenBase
    {
        DemoPanelCtrl mCtrl;
        DemoPanelScreenParam mParam;

        protected override void OnLoadSuccess()
        {
            mCtrl = mCtrlBase as DemoPanelCtrl;
            mParam = mOpenParam as DemoPanelScreenParam;

            
        }

    }
}