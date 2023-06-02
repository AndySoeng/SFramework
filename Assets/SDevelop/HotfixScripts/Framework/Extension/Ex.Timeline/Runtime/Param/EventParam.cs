using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Ex
{
    [Serializable]
    public class EventParam : Marker, INotification, INotificationOptionProvider
    {
        #region INotificationOptionProvider implementation

        [SerializeField] bool m_Retroactive;
        [SerializeField] bool m_EmitOnce;

        /// <summary>
        /// Use retroactive to emit the signal if playback starts after the SignalEmitter time.
        /// </summary>
        public bool retroactive
        {
            get { return m_Retroactive; }
            set { m_Retroactive = value; }
        }

        /// <summary>
        /// Use emitOnce to emit this signal once during loops.
        /// </summary>
        public bool emitOnce
        {
            get { return m_EmitOnce; }
            set { m_EmitOnce = value; }
        }


        PropertyName INotification.id
        {
            get
            {
                if (eventName != EventName.NONE)
                {
                    return new PropertyName(eventName.ToString());
                }

                return new PropertyName(string.Empty);
            }
        }

        NotificationFlags INotificationOptionProvider.flags
        {
            get
            {
                return (retroactive ? NotificationFlags.Retroactive : default(NotificationFlags)) |
                       (emitOnce ? NotificationFlags.TriggerOnce : default(NotificationFlags)) |
                       NotificationFlags.TriggerInEditMode;
            }
        }

        #endregion

        [Header("事件参数")]
        [SerializeField]
        public EventName eventName;
        [SerializeField]
        public EventParamPack eventParamPack;
    }
}