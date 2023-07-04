using UnityEngine;

public class RWCONTROLTest : MonoBehaviour
{
    
    public static string IP_RWCONTROL = "192.168.31.88";
    public static int PORT_RWCONTROL = 8080;

    RWCONTROL_UDPSender _RWCONTROLUDPSender = new RWCONTROL_UDPSender(IP_RWCONTROL, PORT_RWCONTROL);
    
    void LateUpdate()
    {
        _RWCONTROLUDPSender.Send(
            RWCONTROL_Protol.GetBytes_3AxisUnity
                (transform.position.y, transform.eulerAngles.z, transform.eulerAngles.x,
                    RWCONTROL_Protol.Speed.Middle));
    }
}