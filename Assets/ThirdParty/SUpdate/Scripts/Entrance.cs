using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace SFramework
{
    public class Entrance : MonoBehaviour
    {
        private async void Awake()
        {
            Instantiate(Resources.Load<GameObject>("SUpdateUI"), transform).GetComponent<SUpdate>().UpdateRes(OnUpdateResComplete);
        }

        private async void OnUpdateResComplete()
        {
            GetComponentInChildren<EventSystem>().gameObject.SetActive(false);
            await Addressables.InstantiateAsync("BuildFramework.prefab");
        }
    }
}