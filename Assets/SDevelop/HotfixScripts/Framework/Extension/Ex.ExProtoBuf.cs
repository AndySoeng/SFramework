using System.IO;
using Google.Protobuf;

namespace Ex
{
    public static class ExProtobuf
    {
        public static byte[] Serialize(IMessage iMessage)
        {
            using (MemoryStream ms = new MemoryStream())

            {
                iMessage.WriteTo(ms);

                byte[] result = new byte[ms.Length];

                ms.Position = 0;

                ms.Read(result, 0, result.Length);

                return result;
            }

            #region 直接转换

            //return iMessage.ToByteArray();

            #endregion
        }

        public static T Deserialize<T>(byte[] b) where T : IMessage, new()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(b, 0, b.Length);
                ms.Position = 0;
                //TODO 看了以下，这个MessageParser应该是可以复用的
                MessageParser<T> iMessageParser = new MessageParser<T>(() => new T());
                return iMessageParser.ParseFrom(ms);
            }

            #region 直接转换

            // MessageParser<T> iMessageParser = new MessageParser<T>(() => new T());
            // return iMessageParser.ParseFrom(b);

            #endregion
        }
    }
}