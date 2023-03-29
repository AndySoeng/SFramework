using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace Ex
{
    public static class ExEventTrigger
    {
        /// <summary>
        /// UI添加EventTrigger事件触发器,并绑定回调事件
        /// </summary>
        /// <param name="insObject">UI</param>
        /// <param name="eventType">事件触发器类型</param>
        /// <param name="callback">回调事件</param>
        public static void Add(GameObject insObject, EventTriggerType eventType, UnityAction<BaseEventData> unityAction)
        {
            //获取实例化按钮下的EventTrigger组件,准备为其添加交互事件
            EventTrigger eventTrigger = insObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = insObject.AddComponent<EventTrigger>();
            }

            //判断事件入口注册的方法数量,实例化delegates
            if (eventTrigger.triggers.Count == 0)
            {
                eventTrigger.triggers = new List<EventTrigger.Entry>();
            }

            EventTrigger.Entry entry = eventTrigger.triggers.Find(item => item.eventID == eventType);
            if (entry == null)
            {
                //实例事件入口,定义所要绑定的事件类型 
                entry = new EventTrigger.Entry
                {
                    //设置监听事件类型
                    eventID = eventType
                };
                //将事件入口添加给EventTrigger组件
                eventTrigger.triggers.Add(entry);
            }

            //设置回调函数
            entry.callback.AddListener(unityAction);
        }


        /// <summary>
        /// 清除UI上EventTrigger组件的所有事件入口
        /// </summary>
        /// <param name="insObject"></param>
        public static void RemoveAll(GameObject insObject)
        {
            EventTrigger eventTrigger = insObject.GetComponent<EventTrigger>();
            if (eventTrigger)
            {
                eventTrigger.triggers.RemoveAll(p => true);
            }
        }

        /// <summary>
        /// 清除UI上EventTrigger组件的指定类型的事件入口
        /// </summary>
        /// <param name="insObject"></param>
        /// <param name="eventType"></param>
        public static void Remove(GameObject insObject, EventTriggerType eventType)
        {
            EventTrigger eventTrigger = insObject.GetComponent<EventTrigger>();
            if (eventTrigger)
            {
                eventTrigger.triggers.RemoveAll(p => p.eventID == eventType);
            }
        }

        /// <summary>
        /// 清除UI上EventTrigger组件的指定类型的事件入口的指定事件
        /// </summary>
        /// <param name="insObject"></param>
        /// <param name="eventType"></param>
        /// <param name="unityAction"></param>
        public static void Remove(GameObject insObject, EventTriggerType eventType, UnityAction<BaseEventData> unityAction)
        {
            EventTrigger eventTrigger = insObject.GetComponent<EventTrigger>();
            if (eventTrigger)
            {
                EventTrigger.Entry entry = eventTrigger.triggers.Find(item => item.eventID == eventType);
                if (entry != null)
                {
                    entry.callback.RemoveListener(unityAction);
                }
            }
        }
    }
}
