using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SFramework
{
    public class SAAUpdateRes : MonoBehaviour
    {
        public SAAUpdateResUI m_SAAUpdateResUI;

        private AsyncOperationHandle<IResourceLocator> _initializeAsync;
        private AsyncOperationHandle<List<string>> _checkForCatalogUpdates;
        private AsyncOperationHandle<List<IResourceLocator>> _updateCatalogs;
        private AsyncOperationHandle<long> _downloadSize;
        private AsyncOperationHandle _downloadHandle;


        public async UniTask UpdateRes(Action completeAction)
        {
            List<object> needUpdateKeys = new List<object>();

            _initializeAsync = Addressables.InitializeAsync(false);
            await _initializeAsync.Task;
            if (_initializeAsync.Status == AsyncOperationStatus.Failed)
                Debug.LogError("SAAUpdateRes InitializeAsync Failed . " + _initializeAsync.OperationException);

            _checkForCatalogUpdates = Addressables.CheckForCatalogUpdates(false);
            await _checkForCatalogUpdates.Task;
            if (_checkForCatalogUpdates.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("SAAUpdateRes CheckForCatalogUpdates Failed . " + _checkForCatalogUpdates.OperationException);
                //此处可能失败
                UpdateResFailed(completeAction);
                return;
            }

            Debug.Log("SAAUpdateRes CheckForCatalogUpdates Count : " + _checkForCatalogUpdates.Result.Count);

            if (_checkForCatalogUpdates.Result.Count > 0)
            {
                _updateCatalogs = Addressables.UpdateCatalogs(_checkForCatalogUpdates.Result, false);
                await _updateCatalogs.Task;
                if (_updateCatalogs.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError("SAAUpdateRes UpdateCatalogs Failed .  " + _updateCatalogs.OperationException);
                    //此处可能失败
                    UpdateResFailed(completeAction);
                    return;
                }

                Debug.Log("SAAUpdateRes UpdateCatalogs Count : " + _updateCatalogs.Result.Count);
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
            Debug.Log("AllKeys Count : " + needUpdateKeys.Count);

            _downloadSize = Addressables.GetDownloadSizeAsync(needUpdateKeys);
            await _downloadSize.Task;
            if (_downloadSize.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("SAAUpdateRes GetDownloadSizeAsync Failed .  " + _downloadSize.OperationException);
            }

            Debug.Log("SAAUpdateRes GetDownloadSizeAsync DownloadSize : " + _downloadSize.Result);
            long needDownloadSize = _downloadSize.Result;
            long downloadedSize = 0;

            if (needDownloadSize > 0)
            {
                _downloadHandle = Addressables.DownloadDependenciesAsync(needUpdateKeys, Addressables.MergeMode.Union, false);
                while (!_downloadHandle.IsDone)
                {
                    //此循环是一定会等待到_downloadHandle.PercentComplete为1的，因为PercentComplete为所有子操作的加权进度
                    //当_downloadHandle.PercentComplete为1时_downloadHandle.Status才会Succeeded
                    //_downloadHandle.GetDownloadStatus().Percent会提前为1，但其为真实下载进度
                    await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));
                    downloadedSize = _downloadHandle.GetDownloadStatus().DownloadedBytes;
                    m_SAAUpdateResUI.SetProgress(_downloadHandle.GetDownloadStatus().Percent, needDownloadSize, downloadedSize);
                }

                if (_downloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError("SAAUpdateRes DownloadDependenciesAsync Failed .  " + _downloadHandle.OperationException);
                    //此处可能失败
                    UpdateResFailed(completeAction);
                    return;
                }

                Debug.Log("SAAUpdateRes DownloadDependenciesAsync PercentComplete : " + _downloadHandle.PercentComplete);
            }
            else
            {
                m_SAAUpdateResUI.SetProgress(1);
            }

            ReleaseAllHandle();
            Debug.Log("SAAUpdateRes UpdateRes End . ");

            await UpdateResSucceeded(completeAction);
        }

        private void ReleaseAllHandle()
        {
            if (_downloadHandle.IsValid()) Addressables.Release(_downloadHandle);
            if (_downloadSize.IsValid()) Addressables.Release(_downloadSize);
            if (_updateCatalogs.IsValid()) Addressables.Release(_updateCatalogs);
            if (_checkForCatalogUpdates.IsValid()) Addressables.Release(_checkForCatalogUpdates);
            if (_initializeAsync.IsValid()) Addressables.Release(_initializeAsync);
        }

        private async UniTask UpdateResSucceeded(Action completeAction)
        {
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));
            completeAction?.Invoke();
        }

        private void UpdateResFailed(Action completeAction)
        {
            ReleaseAllHandle();
            Action actionConfrim = async () =>
            {
                m_SAAUpdateResUI.ResetProgress();
                await UpdateRes(completeAction);
            };
            Action actionCancel = () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            };
            m_SAAUpdateResUI.ShowModalWindow("提示", "资源下载失败，请检查网络后重新尝试。", actionConfrim, actionCancel);
        }

        private void OnDestroy()
        {
            ReleaseAllHandle();
        }

        private void OnApplicationQuit()
        {
            ReleaseAllHandle();
        }
    }
}