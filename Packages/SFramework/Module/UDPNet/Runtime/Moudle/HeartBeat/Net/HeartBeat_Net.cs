using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

namespace GameServers.Module
{
    public class HeartBeat_Net : Singleton<HeartBeat_Net>
    {
        public HeartBeat_Net()
        {
            //将操作码以及事件添加到MessageCenter中的字典
            MessageCenter.Instance.AddObserver(OperateCode.HEARTBEAT_REQ, HEARTBEAT_REQ);
            MessageCenter.Instance.AddObserver(OperateCode.HEARTBEAT_REP, HEARTBEAT_REP);
        }

        private static Dictionary<string, float> dic_HeartBeatTime = new Dictionary<string, float>();
        public event Action<string, IPEndPoint> OnConnected;
        public event Action<string> OnDisconnected;
        private float DisConnectTime = 1.5f;

        /// <summary>
        /// 服务端接到客户端的心跳包请求
        /// </summary>
        /// <param name="udpMessage"></param>
        public void HEARTBEAT_REQ(UDPMessage udpMessage)
        {
            HeartBeat_Dto heartBeatDto = Newtonsoft.Json.JsonConvert.DeserializeObject<HeartBeat_Dto>(udpMessage.Pack.Data.ToString());
            if (dic_HeartBeatTime.ContainsKey(heartBeatDto.device))
                dic_HeartBeatTime[heartBeatDto.device] = Time.realtimeSinceStartup;
            else
            {
                dic_HeartBeatTime.Add(heartBeatDto.device, Time.realtimeSinceStartup);
                OnConnected?.Invoke(heartBeatDto.device, udpMessage.IpEnd);
            }

            MessageCenter.Instance.Send(OperateCode.HEARTBEAT_REP, heartBeatDto, udpMessage.IpEnd);
        }


        public IEnumerator CheckeHeartBeat()
        {
            WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
            List<string> needRemove = new List<string>();
            while (true)
            {
                yield return waitForEndOfFrame;
                needRemove.Clear();
                foreach (var v in HeartBeat_Net.dic_HeartBeatTime)
                {
                    if (Time.realtimeSinceStartup - v.Value > DisConnectTime)
                        needRemove.Add(v.Key);
                }

                for (int i = 0; i < needRemove.Count; i++)
                {
                    HeartBeat_Net.dic_HeartBeatTime.Remove(needRemove[i]);
                    OnDisconnected?.Invoke(needRemove[i]);
                }
            }
        }


        /// <summary>
        /// 客户端收到回复的心跳包
        /// </summary>
        /// <param name="udpMessage"></param>
        public void HEARTBEAT_REP(UDPMessage udpMessage)
        {
            HeartBeat_Dto heartBeatDto = Newtonsoft.Json.JsonConvert.DeserializeObject<HeartBeat_Dto>(udpMessage.Pack.Data.ToString());
        }
    }
}