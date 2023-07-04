using UnityEngine;
using UnityEngine.UI;

namespace Ex.Test
{
    public class EventFire : MonoBehaviour
    {
        public Text txt;

        public void LogPack(EventParamPack pack)
        {
            txt.text = pack.param_Int.ToString();
        }
    }
}