using System;
using UnityEngine;


namespace Ex
{
    public static class ExTime
    {
        /// <summary>
        /// 获取00：00：00格式时间，最高位为小时，最低位为秒
        /// </summary>
        /// <param name="time">时长（秒）</param>
        /// <returns></returns>
        public static string GetTime(float time)
        {
            float h = Mathf.FloorToInt(time / 3600f);
            float m = Mathf.FloorToInt(time / 60f - h * 60f);
            float s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        /// <summary>
        /// 获取00：00：00：00格式时间，最高位为day，最低位为秒
        /// </summary>
        /// <param name="time">时长（秒）</param>
        /// <returns></returns>
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
        /// <param name="bflag">为真时获取10位时间戳(秒),为假时获取13位时间戳（毫秒）。</param>  
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

        /// <summary>
        /// 获取时间差值，00：00：00格式时间，最高位为小时，最低位为秒
        /// </summary>
        /// <param name="start">起始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        public static string GetTimeOffset(DateTime start,DateTime end)
        {
            long offset = end.GetTimeStamp(true) - start.GetTimeStamp(true);
            if (offset<=0)
            {
                return "00:00:00";
            }
            return  GetTime(offset);
        }
    }
}