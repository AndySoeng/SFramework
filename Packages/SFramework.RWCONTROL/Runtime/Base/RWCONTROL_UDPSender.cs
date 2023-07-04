using System.Net.Sockets;

public class RWCONTROL_UDPSender
{
    private UdpClient _client;

    public RWCONTROL_UDPSender(string host = "192.168.31.88", int port = 8080)
    {
        _client = new UdpClient();
        _client.Connect(host, port);
    }

    public void Send(byte[] bytes)
    {
        _client.Send(bytes, bytes.Length);
    }

    ~RWCONTROL_UDPSender()
    {
        _client.Close();
        _client.Dispose();
    }
}