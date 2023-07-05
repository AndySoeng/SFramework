using System.Net;

namespace GameServers
{
    public class UDPMessage
    {
        public IPEndPoint IpEnd { get; set; }

        public MessagePack Pack { get; set; }

        public UDPMessage()
        {
        }

        public UDPMessage(IPEndPoint ipEnd, MessagePack pack)
        {
            IpEnd = ipEnd;
            Pack = pack;
        }
    }
}