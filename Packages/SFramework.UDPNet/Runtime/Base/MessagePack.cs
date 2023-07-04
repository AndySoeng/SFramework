using System;
using System.Net;

namespace GameServers
{
    public class MessagePack
    {
        /// <summary>
        /// 操作码
        /// </summary>
        public short OpCode { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; set; }

        public MessagePack()
        {
        }

        public MessagePack( short opCode, object data)
        {
            OpCode = opCode;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}