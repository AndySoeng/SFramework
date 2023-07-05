using System;
using Ex;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 用来检测物体指定物体是否环绕了一周
/// 例如：检测玩家相机环顾四周
/// </summary>
public class LookAroundDetection : MonoBehaviour
{
    #region Private Fields
    
    [InlineButton("ResetLookAround", "重置环顾")] 
    [InlineButton("StartLookAround", "开始环顾")]
    [SerializeField]
    private Transform detectionTarget;

    /// <summary>
    /// 环绕进度超过此值时，会被认为已经环顾
    /// </summary>
    [LabelText("环顾判定值")] [Range(0.8f, 1)] [SerializeField]
    private float progressThreshold = 0.98f;

    private enum Dir
    {
        None,
        Left,
        Right,
    }

    private Dir _curDirection = Dir.None;

    /// <summary>
    /// 主相机的初始化 Y 轴旋转角度
    /// </summary>
    private float _targetInitialYAngle = 0;


    /// <summary>
    /// 玩家向右观察完成
    /// </summary>
    private bool _hasLookedAroundR = false;

    /// <summary>
    /// 玩家向左观察完成
    /// </summary>
    private bool _hasLookedAroundL = false;

    #endregion

    #region Public Fields

    [ToggleGroup("needUI")]
    public bool needUI = true;

    [ToggleGroup("needUI")] 
    public GameObject uiPanel;

    [ToggleGroup("needUI")] 
    public Image progressSliderL, progressSliderR;

    
    /// <summary>
    /// 向右环视的进度值
    /// </summary>
    
    public float ProgressR { private set; get; }= 0f;

    /// <summary>
    /// 向左环视的进度值
    /// </summary>
    public float ProgressL { private set; get; } = 0f;

    /// <summary>
    /// 完成右环顾事件
    /// </summary>
    public UnityEvent OnCompleteLookAroundRight;

    /// <summary>
    /// 完成左环顾事件
    /// </summary>
    public UnityEvent OnCompleteLookAroundLeft;

    /// <summary>
    /// 完成左右环顾事件
    /// </summary>
    public UnityEvent OnCompleteLookAround;

    #endregion


    #region Private Methods

    private void Update()
    {
        if (detectionTarget == null || GetHasLookedAround())
            return;
        SetProgressValue();
        SetUIProgressToUI(ProgressL, ProgressR);
        
    }


    private void SetProgressValue()
    {
        float targetRotation = detectionTarget.eulerAngles.y;


        float deltaRotation = CalculateYRotationDiff(targetRotation, _targetInitialYAngle);
        float progressAngle = deltaRotation / 360.0f;


        float progress = 0;
        if (0 > progressAngle && progressAngle > -0.05f)
        {
            _curDirection = Dir.Left;
        }

        if (0 < progressAngle && progressAngle < 0.05f)
        {
            _curDirection = Dir.Right;
        }


        if (_curDirection == Dir.Left)
        {
            if (progressAngle > 0)
            {
                progress = 1 - progressAngle;
            }
            else
            {
                progress = Mathf.Abs(progressAngle);
            }
        }

        if (_curDirection == Dir.Right)
        {
            if (progressAngle > 0)
            {
                progress = progressAngle;
            }
            else
            {
                progress = 1 + progressAngle;
            }
        }

        if (_curDirection == Dir.Right)
        {
            if (progress <= 0.5f)
            {
                if (!_hasLookedAroundR)
                {
                    ProgressR = Mathf.Abs(progress * 2) * 1.176f;

                    // 完成右看
                    if (ProgressR >= progressThreshold)
                    {
                        _hasLookedAroundR = true;
                        ProgressR = 1;
                        OnCompleteLookAroundRight?.Invoke();
                    }
                }
            }
            else
            {
                if (!_hasLookedAroundL)
                {
                    //1+progressAngle
                    ProgressL = Mathf.Abs((progress - 0.5f) * 2) * 1.176f;

                    // 完成左看
                    if (ProgressL >= progressThreshold)
                    {
                        _hasLookedAroundL = true;
                        ProgressL = 1;
                        OnCompleteLookAroundLeft?.Invoke();
                    }
                }
            }
        }
        else if (_curDirection == Dir.Left)
        {
            if (progress <= 0.5)
            {
                if (!_hasLookedAroundL)
                {
                    //1+progressAngle
                    ProgressL = Mathf.Abs(progress * 2) * 1.176f;

                    // 完成左看
                    if (ProgressL >= progressThreshold)
                    {
                        _hasLookedAroundL = true;
                        ProgressL = 1;
                        OnCompleteLookAroundLeft?.Invoke();
                    }
                }
            }
            else
            {
                if (!_hasLookedAroundR)
                {
                    ProgressR = Mathf.Abs((progress - 0.5f) * 2) * 1.176f;

                    // 完成右看
                    if (ProgressR >= progressThreshold)
                    {
                        _hasLookedAroundR = true;
                        ProgressR = 1;
                        OnCompleteLookAroundRight?.Invoke();
                    }
                }
            }
        }


        if (GetHasLookedAround())
        {
            OnCompleteLookAround?.Invoke();
        }
    }


    /// <summary>
    /// 获取角度之间的插值
    /// </summary>
    /// <param name="targetAngle"></param>
    /// <param name="currentAngle"></param>
    /// <returns></returns>
    private float CalculateYRotationDiff(float targetAngle, float currentAngle)
    {
        // 计算逆时针旋转和顺时针旋转需要的角度
        float diff1 = targetAngle - currentAngle;
        float diff2 = currentAngle - targetAngle + 360;

        // 选择较小的角度差
        float absDiff1 = Mathf.Abs(diff1);
        float absDiff2 = Mathf.Abs(diff2);
        float minDiff = Mathf.Min(absDiff1, absDiff2);

        // 判断顺逆时针旋转，返回不同的角度差
        if (absDiff1 <= absDiff2)
        {
            if (diff1 >= 0) return minDiff;
            else return -minDiff;
        }
        else
        {
            if (diff2 >= 0) return -minDiff;
            else return minDiff;
        }
    }

    private void SetUIProgressToUI(float l, float r)
    {
        if (needUI)
        {
            progressSliderL.fillAmount = l;
            progressSliderR.fillAmount = r;
        }
    }

    #endregion

    /// <summary>
    /// 获取玩家是否已经环顾四周
    /// </summary>
    public bool GetHasLookedAround()
    {
        return _hasLookedAroundL && _hasLookedAroundR;
    }


    public void StartLookAround(Transform target = null)
    {
        if (this.detectionTarget == null)
        {
            if (target != null)
                this.detectionTarget = target;
            else
                Debug.LogError("必须指定一个非空的环顾检测物");
        }
        
        ResetLookAround();
    }


    /// <summary>
    /// 重置参数
    /// </summary>
    public void ResetLookAround()
    {
        _curDirection = Dir.None;
        _targetInitialYAngle = detectionTarget.eulerAngles.y;
        ProgressR = 0.0f;
        ProgressL = 0.0f;
        _hasLookedAroundR = false;
        _hasLookedAroundL = false;


        if (uiPanel != null)
        {
            uiPanel.SetActive(needUI);
        }
        SetUIProgressToUI(ProgressL, ProgressR);
    }
}