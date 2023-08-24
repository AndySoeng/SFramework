using Cysharp.Threading.Tasks;

namespace SFramework.UI
{
    using SFramework;
    using UnityEngine;

    public class RectMaskPanelScreenParam : UIOpenScreenParameterBase
    {
        public  RectTransform _maskTarget;
    }

    public class RectMaskPanelScreen : UIScreenBase
    {
        RectMaskPanelCtrl mCtrl;
        RectMaskPanelScreenParam mParam;

        protected override async UniTask OnLoadSuccess()
        {
            await base.OnLoadSuccess();
            mCtrl = mCtrlBase as RectMaskPanelCtrl;
            mParam = mOpenParam as RectMaskPanelScreenParam;


            ShowMask(mParam._maskTarget);
        }
        
        

        public void ShowMask(RectTransform target)
        {
            mCtrl._rectMask.Show(target);
        }

        public void HideMask()
        {
            mCtrl._rectMask.Hide();
        }

    }
}