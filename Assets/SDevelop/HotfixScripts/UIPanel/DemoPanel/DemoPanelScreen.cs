using Cysharp.Threading.Tasks;

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

        protected override async UniTask OnLoadSuccess()
        {
            await base.OnLoadSuccess();
            mCtrl = mCtrlBase as DemoPanelCtrl;
            mParam = mOpenParam as DemoPanelScreenParam;
        }

    }
}