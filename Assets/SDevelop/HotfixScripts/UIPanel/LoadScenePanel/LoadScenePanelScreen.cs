using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace SFramework.UI
{
    using SFramework;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;

    public class LoadScenePanelScreenParam : UIOpenScreenParameterBase
    {
        public LoadSceneName mLoadSceneName;
        public LoadSceneMode mode;

        public UnityAction OnComplete;
    }

    public class LoadScenePanelScreen : UIScreenBase
    {
        LoadScenePanelCtrl mCtrl;
        LoadScenePanelScreenParam mParam;


        protected override async UniTask OnLoadSuccess()
        {
            await base.OnLoadSuccess();
            mCtrl = mCtrlBase as LoadScenePanelCtrl;
            mParam = mOpenParam as LoadScenePanelScreenParam;
            mCtrl.txt_Label.text = I2.Loc.LocalizationManager.GetTranslation("sceneloading");

            mCtrl.StartCoroutine(AsyncLoadScene());
        }


        private IEnumerator AsyncLoadScene()
        {
            AsyncOperationHandle<SceneInstance> handle_LoadScene = Addressables.LoadSceneAsync(mParam.mLoadSceneName.ToString(), mParam.mode);
            //AsyncOperation ao = SceneManager.LoadSceneAsync(mParam.mLoadSceneName.ToString(), mParam.mode);
            while (handle_LoadScene.IsDone != true)
            {
                if (mCtrl.slider_processBar != null)
                {
                    mCtrl.slider_processBar.currentPercent = handle_LoadScene.PercentComplete * 100;
                }

                yield return new WaitForEndOfFrame();
            }

            mCtrl.txt_Label.text = I2.Loc.LocalizationManager.GetTranslation("sceneloaded");
            mCtrl.slider_Loop.GetComponent<Animator>().enabled = false;
            mCtrl.slider_processBar.currentPercent = 100;
            mCtrl.slider_Loop.bar.fillAmount = 1;
            yield return new WaitForEndOfFrame();
            GC.Collect();
            Resources.UnloadUnusedAssets();
            mParam.OnComplete?.Invoke();
            SUIManager.Ins.CloseUI<LoadScenePanelScreen>();
        }


        /// <summary>
        /// 加载实验场景使用
        /// </summary>
        /// <param name="nextSceneName"></param>
        /// <param name="mode"></param>
        public static async UniTask<LoadScenePanelScreen> LoadSingleScene(LoadSceneName nextSceneName, UnityAction OnComplete = null)
        {
            UIScreenBase sb = await SUIManager.Ins.OpenUI<LoadScenePanelScreen>(new LoadScenePanelScreenParam()
            {
                mLoadSceneName = nextSceneName, mode = LoadSceneMode.Single,
                OnComplete = OnComplete,
            });
            return sb as LoadScenePanelScreen;
        }
    }
}