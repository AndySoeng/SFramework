using System;

namespace SFramework
{
    using UnityEngine;
    using SFramework;
    using UI;

    [DisallowMultipleComponent]
    public class BuildFramework : MonoBehaviour
    {
        private async void Awake()
        {
            
            //await SDebugger.Init();
            
            await SAudioManager.Init();
            await SUIManager.Init();

            WebglExpData.firstEnterTime = DateTime.Now;
            LoadScenePanelScreen.LoadSingleScene(LoadSceneName.Scene_Main, () =>
            {
                ModalWindowPanelScreen.OpenModalWindowNoTabs("提示", "场景加载完成", true, null, false);
            });
        }
    }
}