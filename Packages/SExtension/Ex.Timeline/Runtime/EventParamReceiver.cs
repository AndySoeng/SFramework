using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Ex
{
    public class EventParamReceiver : MonoBehaviour, INotificationReceiver
    {
        public AssetEventPair[] eventPair;

        [Serializable]
        public class AssetEventPair
        {
            public EventName eventName;
            public ParamEvent events;

            [Serializable]
            public class ParamEvent : UnityEvent<EventParamPack>
            {
            }
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is EventParam eventParam)
            {
                var matches = eventPair.Where(x => x.eventName == eventParam.eventName);
                foreach (var m in matches)
                {
                    m.events.Invoke(eventParam.eventParamPack);
                }
            }
        }
    }
}