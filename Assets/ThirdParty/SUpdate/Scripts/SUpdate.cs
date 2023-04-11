using System;
using System.Collections;
using System.Collections.Generic;
using HybridCLR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace SFramework
{
    public class SUpdate : MonoBehaviour
    {
        [FormerlySerializedAs("m_SUpdateResUI")] [SerializeField]
        private SUpdateUI mSUpdateUI;

        private AsyncOperationHandle<IResourceLocator> _initializeAsync;
        private AsyncOperationHandle<List<string>> _checkForCatalogUpdates;
        private AsyncOperationHandle<List<IResourceLocator>> _updateCatalogs;
        private AsyncOperationHandle<long> _downloadSize;
        private AsyncOperationHandle _downloadHandle;

        public void UpdateRes(Action completeAction)
        {
            StartCoroutine(AAUpdateRes(completeAction));
        }

        private IEnumerator AAUpdateRes(Action completeAction)
        {
            List<object> needUpdateKeys = new List<object>();

            _initializeAsync = Addressables.InitializeAsync(false);
            yield return _initializeAsync;
            if (_initializeAsync.Status == AsyncOperationStatus.Failed)
                Debug.LogError("SUpdateRes InitializeAsync Failed . " + _initializeAsync.OperationException);

            _checkForCatalogUpdates = Addressables.CheckForCatalogUpdates(false);
            yield return _checkForCatalogUpdates;
            if (_checkForCatalogUpdates.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("SUpdateRes CheckForCatalogUpdates Failed . " +
                               _checkForCatalogUpdates.OperationException);
                //此处可能失败
                UpdateResFailed(completeAction);
                yield break;
            }

            Debug.Log("SUpdateRes CheckForCatalogUpdates Count : " + _checkForCatalogUpdates.Result.Count);

            if (_checkForCatalogUpdates.Result.Count > 0)
            {
                _updateCatalogs = Addressables.UpdateCatalogs(_checkForCatalogUpdates.Result, false);
                yield return _updateCatalogs;
                if (_updateCatalogs.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError("SUpdateRes UpdateCatalogs Failed .  " + _updateCatalogs.OperationException);
                    //此处可能失败
                    UpdateResFailed(completeAction);
                    yield break;
                }

                Debug.Log("SUpdateRes UpdateCatalogs Count : " + _updateCatalogs.Result.Count);
                List<IResourceLocator> assets = _updateCatalogs.Result;
                for (int i = 0; i < assets.Count; i++)
                {
                    needUpdateKeys.AddRange(assets[i].Keys);
                }
            }
            else
            {
                needUpdateKeys.AddRange(_initializeAsync.Result.Keys);
            }


            //开始获取大小及更新流程
            Debug.Log("SUpdateRes AllKeys Count : " + needUpdateKeys.Count);

            _downloadSize = Addressables.GetDownloadSizeAsync((IEnumerable)needUpdateKeys);
            yield return _downloadSize;
            if (_downloadSize.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("SUpdateRes GetDownloadSizeAsync Failed .  " + _downloadSize.OperationException);
            }

            Debug.Log("SUpdateRes GetDownloadSizeAsync DownloadSize : " + _downloadSize.Result);
            long needDownloadSize = _downloadSize.Result;
            long downloadedSize = 0;

            if (needDownloadSize > 0)
            {
                _downloadHandle = Addressables.DownloadDependenciesAsync((IEnumerable)needUpdateKeys,
                    Addressables.MergeMode.Union, false);

                while (!_downloadHandle.IsDone)
                {
                    //此循环是一定会等待到_downloadHandle.PercentComplete为1的，因为PercentComplete为所有子操作的加权进度
                    //当_downloadHandle.PercentComplete为1时_downloadHandle.Status才会Succeeded
                    //_downloadHandle.GetDownloadStatus().Percent会提前为1，但其为真实下载进度
                    yield return new WaitForEndOfFrame();
                    downloadedSize = _downloadHandle.GetDownloadStatus().DownloadedBytes;
                    mSUpdateUI.SetProgress(_downloadHandle.GetDownloadStatus().Percent, needDownloadSize,
                        downloadedSize);
                }

                if (_downloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError("SUpdateRes DownloadDependenciesAsync Failed .  " +
                                   _downloadHandle.OperationException);
                    //此处可能失败
                    UpdateResFailed(completeAction);
                    yield break;
                }

                Debug.Log("SUpdateRes DownloadDependenciesAsync PercentComplete : " +
                          _downloadHandle.PercentComplete);
            }
            else
            {
                mSUpdateUI.SetProgress(1);
            }


            ReleaseAllHandle();
            Debug.Log("SUpdateRes UpdateRes End . ");

            yield return LoadDll();
            UpdateResSucceeded(completeAction);
        }

        private void ReleaseAllHandle()
        {
            if (_downloadHandle.IsValid()) Addressables.Release(_downloadHandle);
            if (_downloadSize.IsValid()) Addressables.Release(_downloadSize);
            if (_updateCatalogs.IsValid()) Addressables.Release(_updateCatalogs);
            if (_checkForCatalogUpdates.IsValid()) Addressables.Release(_checkForCatalogUpdates);
            if (_initializeAsync.IsValid()) Addressables.Release(_initializeAsync);
        }


        private void UpdateResSucceeded(Action completeAction)
        {
            completeAction?.Invoke();
        }

        private void UpdateResFailed(Action completeAction)
        {
            ReleaseAllHandle();
            mSUpdateUI.ShowModalWindow("提示", "资源下载失败，请检查网络后重新尝试。", () =>
            {
                mSUpdateUI.ResetProgress();
                UpdateRes(completeAction);
            }, () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            });
        }

        private void OnDestroy()
        {
            ReleaseAllHandle();
        }

        private void OnApplicationQuit()
        {
            ReleaseAllHandle();
        }


        private static IEnumerator LoadDll()
        {
#if !UNITY_EDITOR
            var updateConfig = Addressables.LoadAssetAsync<SUpdateConfig>("SFrameworkConfig.asset");
            yield return updateConfig;
            yield return LoadMetadataForAOTAssemblies(updateConfig.Result.aotAndHotFixAssembliesDstDir, updateConfig.Result.aotMetaAssemblyFiles);
            yield return LoadHotfixAssemblies(updateConfig.Result.aotAndHotFixAssembliesDstDir, updateConfig.Result.hotfixAssemblyFiles);
            Debug.Log("Load Hotfix DLL End . ");
#else
            yield return null;
#endif
        }


        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static IEnumerator LoadMetadataForAOTAssemblies(string path, List<string> aotMetaAssemblyFiles)
        {
            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in aotMetaAssemblyFiles)
            {
                var dll = Addressables.LoadAssetAsync<TextAsset>($"{path}/{aotDllName}.bytes");
                yield return dll;
                byte[] dllBytes = dll.Result.bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }


        private static IEnumerator LoadHotfixAssemblies(string path, List<string> hotfixAssemblies)
        {
            foreach (var hotfixDllName in hotfixAssemblies)
            {
                var dll = Addressables.LoadAssetAsync<TextAsset>($"{path}/{hotfixDllName}.bytes");
                yield return dll;
                System.Reflection.Assembly.Load(dll.Result.bytes);
                Debug.Log($"LoadHotfixAssemblies:{hotfixDllName}. ");
            }
        }
    }
}