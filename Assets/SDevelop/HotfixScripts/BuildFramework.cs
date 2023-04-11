using System;
namespace SFramework
{
    using UnityEngine;
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
            await LoadScenePanelScreen.LoadSingleScene(LoadSceneName.Scene_Main,
                OnMainSceneComplete);

        }

        private async void OnMainSceneComplete()
        {
            await ModalWindowPanelScreen.OpenModalWindowNoTabs("提示", "场景加载完成", true, () => { }, false);
        }
    }
}