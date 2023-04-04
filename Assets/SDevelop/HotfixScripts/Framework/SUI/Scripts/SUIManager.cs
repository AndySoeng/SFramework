using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SFramework
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.SceneManagement;

    public class SUIManager : MonoSingleton<SUIManager>
    {
        public GameObject uiRoot;

        [ReadOnly] [ShowInInspector]
        // UI列表缓存
        Dictionary<Type, UIScreenBase> m_TypeScreens = new Dictionary<Type, UIScreenBase>();

        private Dictionary<Type, AsyncOperationHandle<GameObject>> m_CtrlHandle = new Dictionary<Type, AsyncOperationHandle<GameObject>>();

        private int mUIOpenOrder = 0; // UI打开时的Order值 用来标识界面层级顺序

        // uicamera
        Camera uiCamera;
        private Canvas canvas;

        public static Camera UiCamera
        {
            get => Ins.uiCamera;
        }


        public static Vector3 MouseWorldPosition
        {
            get
            {
                Vector3 uipos = Ins.uiCamera.ScreenToWorldPoint(Input.mousePosition);
                return uipos;
            }
        }

        public static Vector3 MouseScreenPosition
        {
            get { return Input.mousePosition; }
        }


        protected override async UniTask OnInit()
        {
            // 初始化UI根节点
            uiRoot= await Addressables.InstantiateAsync("UIRoot.prefab", transform);

            uiRoot.transform.position = Vector3.zero;

            uiCamera = uiRoot.GetComponentInChildren<Camera>();

            SceneManager.sceneLoaded += delegate(Scene arg0, LoadSceneMode mode) { Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(uiCamera); };
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(uiCamera);
        }

        /// <summary>
        ///  UI打开入口没有判断条件直接打开
        /// </summary>
        private void OpenUI(Type type, UIOpenScreenParameterBase param = null, Action<UIScreenBase> onOpen = null)
        {
            UIScreenBase sb = GetUI(type);
            mUIOpenOrder++;

            // 如果已有界面,则不执行任何操作
            if (sb != null)
            {
                onOpen?.Invoke(sb);
            }

            //sb = (ScreenBase) Activator.CreateInstance(type, param);


            sb = (UIScreenBase)Activator.CreateInstance(type);

            string prefabName = "UI"+type.Name.Substring(0, type.Name.Length - "Screen".Length);
            Addressables.InstantiateAsync(prefabName, transform).Completed += (obj) =>
            {
                sb.PanelLoadComplete(transform, uiCamera, param, obj.Result, mUIOpenOrder);
                m_TypeScreens.Add(type, sb);
                m_CtrlHandle.Add(type,obj);
                onOpen?.Invoke(sb);
            };
        }

        /// <summary>
        ///  UI打开入口没有判断条件直接打开
        /// </summary>
        public void OpenUI<TScreen>(UIOpenScreenParameterBase param = null, Action<UIScreenBase> onOpen = null) where TScreen : UIScreenBase
        {
            Type type = typeof(TScreen);
            OpenUI(type, param, sb => { onOpen?.Invoke((TScreen)sb); });
        }

        /// <summary>
        /// UI外部调用的获取接口
        /// </summary>
        private UIScreenBase GetUI(Type type)
        {
            if (!typeof(UIScreenBase).IsAssignableFrom(type)) return default;
            UIScreenBase sb = null;
            if (m_TypeScreens.TryGetValue(type, out sb))
                return sb;
            return null;
        }

        /// <summary>
        /// UI外部调用的获取接口
        /// </summary>
        public TScreen GetUI<TScreen>() where TScreen : UIScreenBase
        {
            Type type = typeof(TScreen);

            return (TScreen)GetUI(type);
        }

        /// <summary>
        /// UI外部调用的关闭接口
        /// </summary>
        private bool CloseUI(Type type)
        {
            UIScreenBase sb = GetUI(type);
            if (sb != null)
            {
                if (type == typeof(UIScreenBase)) // 标尺界面是测试界面 不用关闭
                    return false;
                else
                    sb.OnClose();
                return true;
            }

            return false;
        }

        /// <summary>
        /// UI外部调用的关闭接口
        /// </summary>
        public bool CloseUI<TScreen>() where TScreen : UIScreenBase
        {
            Type type = typeof(TScreen);
            return CloseUI(type);
        }


        public void CloseAllUI()
        {
            // 销毁会从容器中删除 不能用正常遍历方式
            List<Type> keys = new List<Type>(m_TypeScreens.Keys);
            foreach (var k in keys)
            {
                if (k == typeof(UIScreenBase)) // 标尺界面是测试界面 不用关闭
                {
                    continue;
                }

                if (m_TypeScreens.ContainsKey(k))
                    m_TypeScreens[k].OnClose();
            }
        }


        /// <summary>
        /// UI移除时候自动处理的接口 一般不要手动调用
        /// </summary>
        public void RemoveUI(UIScreenBase sBase)
        {
            Type type = sBase.GetType();
            if (m_TypeScreens.ContainsKey(type)) // 根据具体需求决定到底是直接销毁还是缓存
                m_TypeScreens.Remove(type);
            
            if (m_CtrlHandle.ContainsKey(type))
            {
                Addressables.ReleaseInstance(m_CtrlHandle[type]);
                m_CtrlHandle.Remove(type);
            }
            
        }
    }
}