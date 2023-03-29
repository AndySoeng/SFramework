using UnityEngine;

public class HotUpdateMain : MonoBehaviour
{
    public string text;

    void Start()
    {
        Debug.Log($"[{GetType().FullName}] 这个热更新脚本挂载在prefab上，打包成ab。通过从ab中实例化prefab成功还原");
        gameObject.AddComponent<CreateByCode>();
        Debug.Log($"[{GetType().FullName}] =======看到此条日志代表你成功运行了示例项目的热更新代码=======");
    }
}