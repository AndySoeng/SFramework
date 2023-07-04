using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GameServers.Module;
using UnityEngine;


namespace GameServers
{
    public class UDPHelper
    {
        public static string IP_SPORTPLATFORM = "192.168.31.88";
        public static int PORT_SPORTPLATFORM = 8080;


        UdpClient udpClient;
        IPEndPoint locatePoint;
        Thread thread_Receive;
        Thread thread_Send;


        /// <summary>
        /// UPDHelper创建构造
        /// remoteIP为空时，表示是服务器端
        /// </summary>
        /// <param name="locatorIP"></param>
        /// <param name="locatorPort"></param>
        /// <param name="remoteIP"></param>
        /// <param name="remotePort"></param>
        public UDPHelper(string locatorIP, int locatorPort, string remoteIP = null, int remotePort = 0)
        {
            if (udpClient != null)
                return;

            IPAddress locateIp = IPAddress.Parse(locatorIP);
            locatePoint = new IPEndPoint(locateIp, locatorPort);
            udpClient = new UdpClient(locatePoint);

            //监听创建好后，就开始接收信息，并创建一个线程
            thread_Receive = new Thread(Receive);
            thread_Receive.IsBackground = true;
            thread_Receive.Start();

            //开启发送线程
            thread_Send = new Thread(Send);
            thread_Send.Start();


            if (!string.IsNullOrEmpty(remoteIP))
            {
                remoteIPPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
                MessageCenter.Instance.StartCoroutine(SendHeatBeat());
            }
        }

        public static IPEndPoint remoteIPPoint { get; private set; }
        

        private IEnumerator SendHeatBeat()
        {
            WaitForSeconds waitOnSeconds = new WaitForSeconds(1);
            while (true)
            {
                yield return waitOnSeconds;
                MessageCenter.Instance.Send(OperateCode.HEARTBEAT_REQ, new HeartBeat_Dto(), remoteIPPoint);
            }
        }

        public void Close()
        {
            
            thread_Send.Abort();
            thread_Receive.Abort();
            udpClient.Close();
            udpClient.Dispose();
        }

        /// <summary>
        /// 接收线程方法
        /// </summary>
        void Receive()
        {
            byte[] data;
            //远端IP
            IPEndPoint fromIpEnd = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    data = udpClient.Receive(ref fromIpEnd);

                    if (data != null)
                    {
                        byte[] deCompressData = Decompress(data);
                        string dataJson = Encoding.UTF8.GetString(deCompressData);
                        MessagePack pack = Newtonsoft.Json.JsonConvert.DeserializeObject<MessagePack>(dataJson);
                        MessageCenter.Instance.receiveQueue.Enqueue(new UDPMessage(fromIpEnd, pack));
                        Debug.Log($"[{locatePoint.Address}:{locatePoint.Port}<--{fromIpEnd.Address}:{fromIpEnd.Port}]: {dataJson}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }


        private void Send()
        {
            while (true)
            {
                if (MessageCenter.Instance.sendQueue.Count > 0)
                {
                    UDPMessage udpMessage = MessageCenter.Instance.sendQueue.Dequeue();
                    string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(udpMessage.Pack);
                    byte[] dataBody = Encoding.UTF8.GetBytes(dataJson);
                    byte[] compressBody = Compress(dataBody);
                    udpClient.Send(compressBody, compressBody.Length, udpMessage.IpEnd);
                }
            }
        }

        /// <summary>
        /// 获取本地IP的方法
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            //获取本地所有IP地址
            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] ip = ipe.AddressList;
            for (int i = 0; i < ip.Length; i++)
            {
                if (ip[i].AddressFamily.ToString().Equals("InterNetwork"))
                {
                    return ip[i].ToString();
                }
            }

            return null;
        }


        public static byte[] Compress(byte[] sendData)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress, true))
                {
                    zipStream.Write(sendData, 0, sendData.Length);
                    zipStream.Close();

                    byte[] compressData = outStream.ToArray();
                    //Debug.Log("[发送压缩前]：" + sendData.Length + "	[发送压缩后]：" + compressData.Length);
                    return compressData;
                }
            }
        }


        public static byte[] Decompress(byte[] inputBytes)
        {
            using (MemoryStream inputStream = new MemoryStream(inputBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress, true))
                    {
                        zipStream.CopyTo(outStream);
                        zipStream.Close();
                        byte[] tempBytes = outStream.ToArray();
                        //Debug.Log("[解压前]：" + inputBytes.Length + "	[压缩后]：" + tempBytes.Length);
                        return tempBytes;
                    }
                }
            }
        }
    }
}