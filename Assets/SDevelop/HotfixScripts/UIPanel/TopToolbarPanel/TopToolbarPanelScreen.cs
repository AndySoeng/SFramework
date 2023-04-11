using Cysharp.Threading.Tasks;

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

        protected override async UniTask OnLoadSuccess()
        {
            await base.OnLoadSuccess();
            mCtrl = mCtrlBase as TopToolbarPanelCtrl;
            mParam = mOpenParam as TopToolbarPanelScreenParam;

            mCtrl.btn_Back.onClick.AddListener(() => { });
        }
    }
}