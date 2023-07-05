using System.Net;
using GameServers;
using GameServers.Module;
using UnityEngine;

public class UDPNetHelperTest : MonoBehaviour
{
    public bool isServer = false;

    private UDPHelper _udpHelper;

    private void Start()
    {
        MessageCenter.CreateInstance();

        if (isServer)
        {
            HeartBeat_Net.Instance.OnConnected += OnConnected;
            HeartBeat_Net.Instance.OnDisconnected += OnDisconnected;
            MessageCenter.Instance.StartCoroutine( HeartBeat_Net.Instance.CheckeHeartBeat());
        }

        _udpHelper = new UDPHelper(UDPHelper.GetIPAddress(), isServer ? 10250 : 10251, isServer ? null : UDPHelper.GetIPAddress(), isServer ? 0 : 10250);
    }

    private void OnConnected(string arg1, IPEndPoint arg2)
    {
        Debug.Log($"客户端:{arg1}已连接，ip:{arg2.Address},端口:{arg2.Port}");
    }

    private void OnDisconnected(string obj)
    {
        Debug.Log($"客户端{obj}已断开连接");
    }


    private void OnDestroy()
    {
        if (isServer)
        {
            HeartBeat_Net.Instance.OnConnected -= OnConnected;
            HeartBeat_Net.Instance.OnDisconnected -= OnDisconnected;
        }

        _udpHelper.Close();
    }
}