using System;

namespace Ex
{
    public enum EventName
    {
        NONE,
        TEST,
    }



    [Serializable]
    public class EventParamPack
    {
        public int param_Int;
    }
}