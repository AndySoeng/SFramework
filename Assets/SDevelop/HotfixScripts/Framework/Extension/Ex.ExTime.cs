using System;
using UnityEngine;


namespace Ex
{
    public static class ExTime
    {
        public static string GetTime(float time)
        {
            float h = Mathf.FloorToInt(time / 3600f);
            float m = Mathf.FloorToInt(time / 60f - h * 60f);
            float s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        public static string GetTimeToDay(float time)
        {
            float d = Mathf.FloorToInt(time / 86400f);
            float h = Mathf.FloorToInt(time / 3600f - d * 86400f);
            float m = Mathf.FloorToInt(time / 60f - h * 60f - d * 86400f);
            float s = Mathf.FloorToInt(time - m * 60f - h * 3600f - d * 86400f);
            return d.ToString("00") + ":" + h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        /// <summary>  
        /// 获取当前时间戳  
        /// </summary>  
        /// <param name="bflag">为真时获取10位时间戳,为假时获取13位时间戳.bool bflag = true</param>  
        /// <returns></returns>  
        public static long GetTimeStamp(this DateTime dateTime, bool bflag)
        {
            TimeSpan ts = dateTime - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long temp;
            if (bflag)
                temp = Convert.ToInt64(ts.TotalSeconds);
            else
                temp = Convert.ToInt64(ts.TotalMilliseconds);

            return temp;
        }
    }
}