using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SFramework
{
    public static class SUpdateCacheClean
    {
        static string cache_ServerDataPath = Directory.GetParent(Application.dataPath)?.ToString() + "/ServerData";

        [MenuItem("Tools/STools/Addressable Cache/Clear ServerData")]
        public static void ClearServerData()
        {
            if (Directory.Exists(cache_ServerDataPath))
            {
                Directory.Delete(cache_ServerDataPath, true);
                Debug.Log("ServerData Cleaned .");
            }
        }

        static string cache_CatalogPath = Application.persistentDataPath + "/com.unity.addressables";

        static string cache_AABundlePath = Application.persistentDataPath.Replace($"{Application.companyName}/{Application.productName}", "") +
                                           $"Unity/{Application.companyName}_{Application.productName}";


        [MenuItem("Tools/STools/Addressable Cache/Clear All Cache")]
        public static void ClearAllCache()
        {
            ClearCatalogCache();
            CleanAABundleCache();
            Debug.Log("All Cache Cleaned .");
        }

        [MenuItem("Tools/STools/Addressable Cache/Clean Catalog Cache")]
        public static void ClearCatalogCache()
        {
            if (Directory.Exists(cache_CatalogPath))
            {
                Directory.Delete(cache_CatalogPath, true);
                Debug.Log("Catalog Cache Cleaned .");
            }
        }


        [MenuItem("Tools/STools/Addressable Cache/Clean AABundle Cache")]
        public static void CleanAABundleCache()
        {
            if (Directory.Exists(cache_AABundlePath))
            {
                Directory.Delete(cache_AABundlePath, true);
                Debug.Log("AABundle Cache Cleaned .");
            }
        }
    }
}