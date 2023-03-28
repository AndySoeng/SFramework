using HybridCLR;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadDll : MonoBehaviour
{
    void Start()
    {
        StartGame();
    }


    async UniTask StartGame()
    {
         await LoadMetadataForAOTAssemblies();
#if !UNITY_EDITOR
        var dll_Hotfix = await Addressables.LoadAssetAsync<TextAsset>("Hotfix.Develop.dll.bytes").Task;
        System.Reflection.Assembly.Load(dll_Hotfix.bytes);
#endif
        GameObject testPrefab = await Addressables.InstantiateAsync("HotUpdatePrefab.prefab").Task;
    }


    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static async UniTask LoadMetadataForAOTAssemblies()
    {
        List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            var dll = await Addressables.LoadAssetAsync<TextAsset>( aotDllName + ".bytes").Task;
            byte[] dllBytes = dll.bytes;
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }
}