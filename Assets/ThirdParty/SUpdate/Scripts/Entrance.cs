using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace SFramework
{
    public class Entrance : MonoBehaviour
    {
        private void Awake()
        {
            Instantiate(Resources.Load<GameObject>("SUpdateUI"), transform).GetComponent<SUpdate>().UpdateRes(() =>
            {
                Addressables.InstantiateAsync("BuildFramework.prefab");
            });
        }
    }
}