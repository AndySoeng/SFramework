

namespace SFramework
{
    using UnityEngine;

    public class UIScreenBase
    {
        public GameObject mPanelRoot = null;
        protected UICtrlBase mCtrlBase;

        // 界面打开的传入参数
        protected UIOpenScreenParameterBase mOpenParam;

        public UICtrlBase CtrlBase
        {
            get => mCtrlBase;
        }

        public void PanelLoadComplete(Transform uiRoot,Camera uiCamera   ,UIOpenScreenParameterBase param ,GameObject ctrl,int openOrder)
        {
            mOpenParam = param;
            mPanelRoot = ctrl;
            
            mCtrlBase = mPanelRoot.GetComponent<UICtrlBase>();
            mPanelRoot.transform.SetParent( uiRoot);
            mPanelRoot.name = mPanelRoot.name.Replace("(Clone)", "");
            
            mCtrlBase.ctrlCanvas = mPanelRoot.GetComponent<Canvas>();
            mCtrlBase.ctrlCanvas.worldCamera = uiCamera;
            mCtrlBase.ctrlCanvas.pixelPerfect = true;
            mCtrlBase.ctrlCanvas.overrideSorting = true;
            mCtrlBase.ctrlCanvas.sortingLayerID = (int) mCtrlBase.sceenPriority;
            mCtrlBase.ctrlCanvas.sortingOrder = openOrder;

            // 调用加载成功方法
            OnLoadSuccess();
        }
        

        // 脚本处理完成
        protected virtual  void OnLoadSuccess()
        {
            
        }
        
        public virtual void OnClose()
        {
            SUIManager.Ins.RemoveUI(this);
        }
        
      
    }
}