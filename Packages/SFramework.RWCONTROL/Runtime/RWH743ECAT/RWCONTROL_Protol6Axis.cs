
using Ex;
public partial class RWCONTROL_Protol
{
   
    /// <summary>
    /// 二十九字节协议（六轴位姿随动控制）。
    /// </summary>
    /// <param name="yHeight">y轴高度</param>
    /// <param name="zEuler">z轴欧拉角</param>
    /// <param name="xEuler">x轴欧拉角</param>
    /// <param name="yEuluer">y轴欧拉角</param>
    /// <param name="zOffset">z轴偏移</param>
    /// <param name="xOffset">x轴偏移</param>
    /// <param name="speed">速度等级</param>
    /// <returns></returns>
    public static byte[] GetBytes_6AxisUnity(float yHeight, float zEuler, float xEuler, float yEuluer, float zOffset, float xOffset, Speed speed = Speed.Middle)
    {
        return GetBytes_6Axis(yHeight * 1000f, zEuler, xEuler, yEuluer, zOffset * 1000f, xOffset * 1000f, speed);
    }


    /// <summary>
    /// 二十九字节协议（六轴位姿随动控制）。
    /// 平台位置即超X,Y,Z轴方向运动的位移。姿态用欧拉角表示，绕三轴旋转，分别为Z轴（升降），A轴（绕X轴旋转，翻滚角），B轴（绕y轴旋转，俯仰角），C轴（绕z轴旋转，偏航角）。
    /// 协议中数据皆为float（浮点型）数据，Z轴数值代表从平台零点上升的高度（毫米/mm），A轴，B轴，C轴数值代表平台运动的角度（度/°）；X轴，Y轴，Z轴数值代表平台运动的位移（毫米/mm）。
    /// </summary>
    /// <param name="zHeight">Z轴数值代表从平台零点上升的高度(毫米/mm)</param>
    /// <param name="AAxisEuler">A轴数值代表平台运动的角度 (度)</param>
    /// <param name="BAxisEuler">B轴数值代表平台运动的角度 (度)</param>
    /// <param name="CAxisEuler">C轴数值代表平台运动的角度 (度)</param>
    /// <param name="XOffset">X轴数值代表平台运动的位移（毫米/mm）</param>
    /// <param name="YOffset">Y轴数值代表平台运动的位移（毫米/mm）</param>
    /// <param name="speed">速度等级分为3级，值为1，2，3。其中1为最慢，3为最快</param>
    /// <returns></returns>
    private static byte[] GetBytes_6Axis(float zHeight, float AAxisEuler, float BAxisEuler, float CAxisEuler, float XOffset, float YOffset, Speed speed = Speed.Middle)
    {
        byte[] bytes = new byte[29];
        bytes[0] = 0xA5;
        bytes[1] = (byte)CMD.姿态随动控制6轴;
        byte[] zHeightBytes = System.BitConverter.GetBytes(zHeight);
        System.Array.Copy(zHeightBytes, 0, bytes, 2, 4);
        byte[] AAxisEulerBytes = System.BitConverter.GetBytes(AAxisEuler);
        System.Array.Copy(AAxisEulerBytes, 0, bytes, 6, 4);
        byte[] BAxisEulerBytes = System.BitConverter.GetBytes(BAxisEuler);
        System.Array.Copy(BAxisEulerBytes, 0, bytes, 10, 4);
        byte[] CAxisEulerBytes = System.BitConverter.GetBytes(CAxisEuler);
        System.Array.Copy(CAxisEulerBytes, 0, bytes, 14, 4);
        byte[] XOffsetBytes = System.BitConverter.GetBytes(XOffset);
        System.Array.Copy(XOffsetBytes, 0, bytes, 18, 4);
        byte[] YOffsetBytes = System.BitConverter.GetBytes(YOffset);
        System.Array.Copy(YOffsetBytes, 0, bytes, 22, 4);
        bytes[26] = (byte)speed;
        ExCRC16_MODBUS.Compute(bytes, 0, 27, out bytes[27], out bytes[28]);
        return bytes;
    }
}