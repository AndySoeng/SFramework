using Ex;

public partial class RWCONTROL_Protol
{
    public enum Speed
    {
        Low = 0x01,
        Middle = 0x02,
        High = 0x03,
    }

    private enum CMD
    {
        陀螺仪姿态随动控制 = 0x17,
        姿态随动控制3轴 = 0x18,
        姿态随动控制6轴 = 0x20,
    }


    /// <summary>
    /// 十七字节协议（三轴姿态随动控制）。
    /// </summary>
    /// <param name="yHeight">y轴高度</param>
    /// <param name="zEuler">z轴欧拉角</param>
    /// <param name="xEuler">x轴欧拉角</param>
    /// <param name="speed">速度等级</param>
    /// <returns></returns>
    public static byte[] GetBytes_3AxisUnity(float yHeight, float zEuler, float xEuler, Speed speed = Speed.Middle)
    {
        return GetBytes_3Axis(yHeight * 1000f, zEuler, xEuler, speed);
    }

    /// <summary>
    /// 十七字节协议（三轴姿态随动控制）。
    /// 三轴平台拥有三个自由度，分别为Z轴(升降)，A轴(绕X轴旋转，翻滚角)，B轴(绕y轴旋转，俯仰角)。
    /// 协议中三个数据皆为foat (浮点型)数据，Z轴数值代表从平台零点上升的高度(毫米/mm)，A轴，B轴数值代表平台运动的角度 (度)
    /// </summary>
    /// <param name="zHeight">Z轴数值代表从平台零点上升的高度(毫米/mm)</param>
    /// <param name="AAxisEuler">A轴数值代表平台运动的角度 (度)</param>
    /// <param name="BAxisEuler">B轴数值代表平台运动的角度 (度)</param>
    /// <param name="speed">速度等级分为3级，值为1，2，3。其中1为最慢，3为最快</param>
    /// <returns></returns>
    private static byte[] GetBytes_3Axis(float zHeight, float AAxisEuler, float BAxisEuler, Speed speed = Speed.Middle)
    {
        byte[] bytes = new byte[17];
        bytes[0] = 0xA5;
        bytes[1] = (byte)CMD.姿态随动控制3轴;
        byte[] zHeightBytes = System.BitConverter.GetBytes(zHeight);
        System.Array.Copy(zHeightBytes, 0, bytes, 2, 4);
        byte[] AAxisEulerBytes = System.BitConverter.GetBytes(AAxisEuler);
        System.Array.Copy(AAxisEulerBytes, 0, bytes, 6, 4);
        byte[] BAxisEulerBytes = System.BitConverter.GetBytes(BAxisEuler);
        System.Array.Copy(BAxisEulerBytes, 0, bytes, 10, 4);
        bytes[14] = (byte)speed;
        //CRC16.GetCrc16(bytes, 0, 15, out bytes[15], out bytes[16]);
        //byte[] modbus = CRC16.ToModbus(bytes);
        ExCRC16_MODBUS.Compute(bytes, 0, 15, out bytes[15], out bytes[16]);
        return bytes;
    }
}