using Cysharp.Threading.Tasks;

namespace SFramework
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;

    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        // 单件子类实例
        private static T _instance;

        public static T Ins
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("MonoSingleton: [" + typeof(T).Name + "] not initialized。");
                }

                return _instance;
            }
        }
        
        public static async   UniTask<T> Init()
        {
            if (_instance == null)
            {
                Type theType = typeof(T);

                _instance = (T) FindObjectOfType(theType);

                if (_instance == null)
                {
                    var go = new GameObject(typeof(T).Name);

                    _instance = go.AddComponent<T>();

                    //挂接到BootObj下
                    GameObject bootObj = GameObject.Find("SFramework");

                    if (bootObj == null)
                    {
                        bootObj = new GameObject("SFramework");
                        DontDestroyOnLoad(bootObj);
                    }

                    go.transform.SetParent(bootObj.transform);

                   await  _instance.GetComponent<MonoSingleton<T>>().OnInit();
                }
                else
                {
                    Debug.LogError("MonoSingleton: [" + typeof(T).Name + "] already exists in the current scene .");
                }
            }
            else
            {
                Debug.LogError("MonoSingleton: [" + typeof(T).Name + "] initialized。");
            }


            return _instance;
        }

        protected virtual async UniTask OnInit()
        {
            await UniTask.Yield();
        }


        protected virtual void Destroy()
        {
            _instance = null;
        }
    }
}