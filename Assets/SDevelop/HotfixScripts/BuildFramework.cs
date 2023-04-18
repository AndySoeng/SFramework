using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SFramework.UI;
using UnityEngine.AddressableAssets;

namespace SFramework
{
    [DisallowMultipleComponent]
    public class BuildFramework : MonoBehaviour
    {
        private async void Awake()
        {
            //await SDebugger.Init();

            I2.Loc.LanguageSourceAsset languageSourceAsset = await Addressables.LoadAssetAsync<I2.Loc.LanguageSourceAsset>("Assets/SDevelop/Localization/Remote I2Languages.asset");
            languageSourceAsset.mSource.Awake();
            await SAudioManager.Init();
            await SUIManager.Init();

            WebglExpData.firstEnterTime = DateTime.Now;
            await LoadScenePanelScreen.LoadSingleScene(LoadSceneName.Scene_Main,
                OnMainSceneComplete);
        }

        private async void OnMainSceneComplete()
        {
            Debug.Log(I2.Loc.LocalizationManager.GetTranslation("Common/Test"));
            await ModalWindowPanelScreen.OpenModalWindowNoTabs(I2.Loc.LocalizationManager.GetTranslation("prompt"),
                I2.Loc.LocalizationManager.GetTranslation("sceneloaded"), true, () => { }, false);
        }
    }
}