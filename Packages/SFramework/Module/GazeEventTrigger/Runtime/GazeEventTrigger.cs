using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 注视组件，通过EventSystem来进行事件的触发，注意触发的物体和UI必须有EventTrigger组件,触发3D物体相机必须有PhysicsRaycaster组件
/// 目前已包含事件：
/// 注视进入 ExecuteEvents.pointerEnterHandler
/// 注视退出 ExecuteEvents.pointerExitHandler
/// 注视时间达到触发点击 ExecuteEvents.pointerClickHandler
/// </summary>
public class GazeEventTrigger : MonoBehaviour
{
    //设置准星填充效果等
    public Image img_Reticle;

    /// <summary>
    /// 触发Click事件的时间
    /// </summary>
    [LabelText("触发点击事件的时间")]
    [Range(1,10)]
    [SerializeField]
    private  float _clickTime = 3;

    /// <summary>
    /// 当前注视已注视时常
    /// </summary>
    private float nowTime = 0;


    void Update()
    {
        RayCast();
    }


    PointerEventData PointerEventData;
    private GameObject curRaycastObj;
    private GameObject lastRaycastObj;

    public void RayCast()
    {
        if (Camera.main == null)
        {
            return;
        }

        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem==null(只要不是一直log就没问题)");
            return;
        }

        if (null == PointerEventData)
        {
            PointerEventData = new PointerEventData(EventSystem.current);
        }

        PointerEventData.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        PointerEventData.delta = Vector2.zero;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(PointerEventData, raycastResults);

        curRaycastObj = null;
        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject == null)
            {
                continue;
            }

            EventTrigger et = raycastResults[i].gameObject.GetComponentInParent<EventTrigger>();

            if (et == null)
            {
                continue;
            }

            curRaycastObj = et.gameObject;
            break;
        }


        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 100, Color.green);


        if (curRaycastObj != null)
        {
            if (curRaycastObj != lastRaycastObj)
            {
                if (lastRaycastObj != null)
                {
                    //视线凝视的上一个物体,完成退出操作
                    ExecuteEvents.Execute(lastRaycastObj, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
                    lastRaycastObj = null;
                }


                //隐藏准星，重置时间
                img_Reticle.fillAmount = 0;
                nowTime = 0;
                ExecuteEvents.Execute(curRaycastObj, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
            }

            nowTime += Time.deltaTime;
            //正在读秒时间
            if (_clickTime > nowTime)
            {
                img_Reticle.fillAmount = nowTime / _clickTime;
            }
            //达到激活条件
            else
            {
                ExecuteEvents.Execute(curRaycastObj, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
                nowTime = 0;
            }
        }
        else
        {
            if (lastRaycastObj != null)
            {
                //视线凝视的上一个物体,完成退出操作
                ExecuteEvents.Execute(lastRaycastObj, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
                lastRaycastObj = null;
                //隐藏准星，重置时间
                img_Reticle.fillAmount = 0;
                nowTime = 0;
            }
        }

        lastRaycastObj = curRaycastObj;
    }
}