namespace SFramework.UI
{
    using SFramework;
    using UnityEngine;

    public class EndPanelScreenParam : UIOpenScreenParameterBase
    {
        /// <summary>
        /// 是否透明，true则透明底，false则为纯黑底
        /// </summary>
        public bool isTransparent;

        /// <summary>
        /// 提示内容
        /// </summary>
        public string content;
    }

    public class EndPanelScreen : UIScreenBase
    {
        EndPanelCtrl mCtrl;
        EndPanelScreenParam mParam;

        protected override void OnLoadSuccess()
        {
            mCtrl = mCtrlBase as EndPanelCtrl;
            mParam = mOpenParam as EndPanelScreenParam;
            mCtrl.txt_EndPanelTXTContent.text = mParam.content;
            if (!mParam.isTransparent)
            {
                mCtrl.ig_Mask.color = Color.black;
            }
        }

    }
}