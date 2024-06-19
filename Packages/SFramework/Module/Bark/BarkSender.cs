using System.Collections;
using System.Security.Cryptography;
using System.Text;
using Ex;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Bark
{
    public class BarkSender : MonoBehaviour
    {
        private static BarkSender ins;

        private void Awake()
        {
            if (ins == null)
            {
                ins = this;
                DontDestroyOnLoad(this);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeInitializeOnLoadMethod()
        {
            GameObject barkObj = new GameObject("Bark");
            barkObj.hideFlags = HideFlags.HideInHierarchy;
            barkObj.AddComponent<BarkSender>();
            BarkSender.Send(GetReadyInfo());
        }


        private static string GetReadyInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine().AppendLine("Environment：");
            sb.AppendLine("项目名称：" + Application.productName);
#if UNITY_5
            sb.AppendLine("项目ID：" + Application.bundleIdentifier);
#endif
#if UNITY_7
            sb.AppendLine("项目ID：" + Application.identifier);
#endif
            sb.AppendLine("项目版本：" + Application.version);
            sb.AppendLine("Unity版本：" + Application.unityVersion);
            sb.AppendLine("公司名称：" + Application.companyName);
            
            sb.AppendLine().AppendLine("Scenes：");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                sb.AppendLine($"场景{i}：" + SceneManager.GetSceneByBuildIndex(i).name);
            
            sb.AppendLine().AppendLine("Screen Information：");
            sb.AppendLine("DPI：" + Screen.dpi);
            sb.AppendLine("分辨率：" + Screen.currentResolution.ToString());
            sb.AppendLine("全屏：" + Screen.fullScreen);

             
            sb.AppendLine().AppendLine("Quality：");
            sb.AppendLine("图形质量：" + QualitySettings.names[QualitySettings.GetQualityLevel()]);
            
            sb.AppendLine().AppendLine("System：");
            sb.AppendLine("操作系统：" + SystemInfo.operatingSystem);
            sb.AppendLine("系统内存：" + SystemInfo.systemMemorySize + "MB");
            sb.AppendLine("处理器：" + SystemInfo.processorType);
            sb.AppendLine("处理器数量：" + SystemInfo.processorCount);
            sb.AppendLine("显卡：" + SystemInfo.graphicsDeviceName);
            sb.AppendLine("显卡类型：" + SystemInfo.graphicsDeviceType);
            sb.AppendLine("显存：" + SystemInfo.graphicsMemorySize + "MB");
            sb.AppendLine("显卡标识：" + SystemInfo.graphicsDeviceID);
            sb.AppendLine("显卡供应商：" + SystemInfo.graphicsDeviceVendor);
            sb.AppendLine("显卡供应商标识码：" + SystemInfo.graphicsDeviceVendorID);
            sb.AppendLine("设备模式：" + SystemInfo.deviceModel);
            sb.AppendLine("设备名称：" + SystemInfo.deviceName);
            sb.AppendLine("设备类型：" + SystemInfo.deviceType);
            sb.AppendLine("设备标识：" + SystemInfo.deviceUniqueIdentifier);
            
#if UNITY_5
            sb.AppendLine().AppendLine("Memory：");
            sb.AppendLine("总内存：" + UnityEngine.Profiling.Profiler.GetTotalReservedMemory() / 1000000 + "MB");
            sb.AppendLine("已占用内存：" + UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / 1000000 + "MB");
            sb.AppendLine("空闲中内存：" + UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemory() / 1000000 + "MB");
            sb.AppendLine("总Mono堆内存：" + UnityEngine.Profiling.Profiler.GetMonoHeapSize() / 1000000 + "MB");
            sb.AppendLine("已占用Mono堆内存：" + UnityEngine.Profiling.Profiler.GetMonoUsedSize() / 1000000 + "MB");
#endif
#if UNITY_7
            sb.AppendLine().AppendLine("Memory：");
            sb.AppendLine("总内存：" + UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1000000 + "MB");
            sb.AppendLine("已占用内存：" + UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1000000 + "MB");
            sb.AppendLine("空闲中内存：" + UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1000000 + "MB");
            sb.AppendLine("总Mono堆内存：" + UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / 1000000 + "MB");
            sb.AppendLine("已占用Mono堆内存：" + UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 1000000 + "MB");
#endif
            return sb.ToString();
        }

        /// <summary>
        /// 发送Bark消息
        /// </summary>
        /// <param name="title">推送标题</param>
        /// <param name="body">推送内容</param>
        /// <param name="leve">推送中断级别。</param>
        /// <param name="badge">推送角标，可以是任意数字</param>
        /// <param name="autoCopy">iOS14.5以下自动复制推送内容，iOS14.5以上需手动长按推送或下拉推送</param>
        /// <param name="copy">复制推送时，指定复制的内容，不传此参数将复制整个推送内容。</param>
        /// <param name="sound">可以为推送设置不同的铃声</param>
        /// <param name="icon">为推送设置自定义图标，设置的图标将替换默认Bark图标。图标会自动缓存在本机，相同的图标 URL 仅下载一次。</param>
        /// <param name="group">对消息进行分组，推送将按group分组显示在通知中心中。也可在历史消息列表中选择查看不同的群组。</param>
        /// <param name="isArchive">传 1 保存推送，传其他的不保存推送，不传按APP内设置来决定是否保存。</param>
        /// <param name="url">点击推送时，跳转的URL ，支持URL Scheme 和 Universal Link</param>
        public static void Send(string body, string title = "", BarkLevel leve = BarkLevel.active, int badge = -1,
            bool autoCopy = false, string copy = "", string sound = "", string icon = "",
            string group = "", BarkArchive isArchive = BarkArchive.NONE, string url = "")
        {
            JsonData jd = new JsonData();
            jd["body"] = body;
            if (!string.IsNullOrEmpty(title)) jd["title"] = title;
            jd["level"] = leve.ToString();
            if (badge > 0) jd["badge"] = badge;
            if (autoCopy == true) jd["autoCopy"] = 1;
            if (!string.IsNullOrEmpty(copy)) jd["copy"] = copy;
            if (!string.IsNullOrEmpty(sound)) jd["sound"] = sound;
            if (!string.IsNullOrEmpty(icon)) jd["icon"] = icon;
            if (!string.IsNullOrEmpty(group)) jd["group"] = group;
            if (isArchive != BarkArchive.NONE) jd["isArchive"] = isArchive == BarkArchive.Archive ? 1 : 0;
            if (!string.IsNullOrEmpty(url)) jd["url"] = url;

            string ciphertext = ExCrypto_AES.Encrypt(jd.ToJson(), CipherMode.CBC, PaddingMode.PKCS7, "2AgvA2569djaVaHg", "9djaVaHg2AgvA256");
            ins.StartCoroutine(SendWWWForm(ciphertext));
        }

        private static IEnumerator SendWWWForm(string barkCiphertext)
        {
            WWWForm form = new WWWForm();
            form.AddField("ciphertext", barkCiphertext);
            using UnityWebRequest unityWeb = UnityWebRequest.Post("https://api.day.app/mgA2tySbPz3uPUMM4p7yDT", form);
            yield return unityWeb.SendWebRequest();
        }

        /// <summary>
        /// 发送Bark消息
        /// </summary>
        /// <param name="body">推送内容</param>
        /// <returns></returns>
        public static void Send(string body)
        {
            Send(body, "Unity", BarkLevel.passive, -1, false, "", "", "https://avatars.githubusercontent.com/u/426196?s=200&v=4", "Unity", BarkArchive.NONE,
                "http://andysoeng:13000/");
        }
    }
}