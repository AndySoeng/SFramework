using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SamplesVisualization : MonoBehaviour
{
    [SerializeField] private Image[] sampleObjs;

    [Range(1, 2)] [SerializeField] private float YMaxScale = 2;
    [Range(0, 10)] [SerializeField] private float colorMultiplyer = 2;
    [Range(0, 1)] [SerializeField] private float s = 0.8f;
    [Range(0, 1)] [SerializeField] private float v = 1;

    /// <summary>
    /// 仅使用前256个，传多了也没用
    /// </summary>
    /// <param name="samples"></param>
    public void OnSamplesChanged(float[] samples)
    {
        for (int i = 0; i < 256; i++)
        {
            if (i % 4 == 0)
            {
                int ii = i / 4;

                float yScale = Mathf.Clamp(samples[i] * 100, 0, YMaxScale);
                //确保参数小于1f时有0.2的增量，保证没有yScale为0的情况
                yScale = yScale > 1f ? yScale : (yScale + 0.1f);
                sampleObjs[ii].gameObject.transform.DOScale(new Vector3(1.0f, yScale, 1f), 0.5f);
                //hue乘系数后+100可保证当参数为0是显示的是绿色
                float hue = Mathf.Clamp(samples[i] * 360f * colorMultiplyer + 100, 0, 360f) / 360f;
                sampleObjs[ii].GetComponent<Image>().color = Color.HSVToRGB(hue, s, v, true);
            }
        }
    }
}