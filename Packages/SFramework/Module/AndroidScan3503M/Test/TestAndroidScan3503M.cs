using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestAndroidScan3503M : MonoBehaviour
{
    public TMP_Text txt_ScanResult;
    public Button btn_OpenScan;
    public Button btn_CloseScan;
    public Button btn_TriggerScan;

    // Start is called before the first frame update
    void Start()
    {
        AndroidScan3503M.Ins.Subscribe(OnScanResult);
        
        btn_OpenScan.onClick.AddListener(() =>
        {
            AndroidScan3503M.Ins.OpenOrCloseScan(true);
        });
        btn_CloseScan.onClick.AddListener(() =>
        {
            AndroidScan3503M.Ins.OpenOrCloseScan(false);
        });
        btn_TriggerScan.onClick.AddListener(() =>
        {
            AndroidScan3503M.Ins.TriggerScan();
        });
    }

    private void OnScanResult(string result)
    {
        if (string.IsNullOrEmpty(result))
            return;
        txt_ScanResult.text += result + "\n";
        Debug.Log("扫描结果:" + result   +"长度:"+result.Length);
    }
}