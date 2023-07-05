using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameServers.Module
{
    class HeartBeat_Dto : Base_Dto
    {
        public string device;

        public HeartBeat_Dto()
        {
            device = SystemInfo.deviceUniqueIdentifier;
        }
    }
}