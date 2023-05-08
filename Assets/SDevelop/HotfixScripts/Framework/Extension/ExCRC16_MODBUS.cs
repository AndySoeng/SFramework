using UnityEngine;

namespace Ex
{
    public static class ExCRC16_MODBUS
    {
        private const ushort Polynomial = 0xA001;
        private static readonly ushort[] CRCTable = new ushort[256];

        static ExCRC16_MODBUS()
        {
            // 初始化CRC表
            for (ushort i = 0; i < 256; i++)
            {
                ushort crc = 0;
                ushort c = i;
                for (int j = 0; j < 8; j++)
                {
                    if (((crc ^ c) & 0x0001) != 0)
                    {
                        crc = (ushort)((crc >> 1) ^ Polynomial);
                    }
                    else
                    {
                        crc >>= 1;
                    }

                    c >>= 1;
                }

                CRCTable[i] = crc;
            }
        }


        public static ushort Compute(byte[] bytes)
        {
            return Compute(bytes, 0, bytes.Length);
        }

        public static ushort Compute(byte[] bytes, int startIndex, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = startIndex; i < length - startIndex; i++)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ CRCTable[index]);
            }

            return crc;
        }

        public static ushort Compute(byte[] bytes, out byte highByte, out byte lowByte)
        {
            return Compute(bytes, 0, bytes.Length, out highByte, out lowByte);
        }

        public static ushort Compute(byte[] bytes, int index, int length, out byte highByte, out byte lowByte)
        {
            ushort crc = Compute(bytes, index, length);
            highByte = (byte)(crc >> 8);
            lowByte = (byte)(crc & 0xFF);
            return crc;
        }


        public static bool Validate(byte[] bytes)
        {
            if (bytes.Length < 2)
            {
                return false; // 数据长度不足，无法进行校验
            }

            ushort receivedCrc = (ushort)((bytes[bytes.Length - 2] << 8) | bytes[bytes.Length - 1]);
            ushort calculatedCrc = Compute(bytes, 0, bytes.Length - 2);

            return receivedCrc == calculatedCrc;
        }
    }
}