using UnityEngine.Timeline;

namespace Ex
{
    [TrackColor(1, 0, 0)]
//绑定对象的类型
    [TrackBindingType(typeof(EventParamReceiver))]
    public class EventTrack : TrackAsset
    {
    }
}