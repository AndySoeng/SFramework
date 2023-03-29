using System;

namespace SFramework
{
    using UnityEngine;
    using SFramework;
    using UI;

    [DisallowMultipleComponent]
    public class Init : MonoBehaviour
    {
        private void Awake()
        {
            WebglExpData.firstEnterTime = DateTime.Now;
#if DEBUG
            SDebugger.Init();
#endif
            SAudioManager.Init();
            SUIManager.Init();


            LoadScenePanelScreen.LoadSingleScene(LoadSceneName.Scene_Main, () => { ModalWindowPanelScreen.OpenModalWindowNoTabs("提示", "场景加载完成", true, null, false); });

        }
    }
}