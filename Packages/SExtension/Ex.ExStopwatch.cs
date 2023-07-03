using System.Diagnostics;
using Unity.VisualScripting;

namespace Ex
{
    public static class ExStopwatch
    {
        public static Stopwatch m_Stopwatch { get; private set; } = new Stopwatch();


        public static void Start(bool reset = true)
        {
            if (m_Stopwatch == null)
                m_Stopwatch = new Stopwatch();
            
            if (reset)
                m_Stopwatch.Restart();
            else
                m_Stopwatch.Start();
        }

        public static void Stop(string format = null)
            {
                m_Stopwatch.Stop();
                if (string.IsNullOrEmpty(format))
                    UnityEngine.Debug.Log(string.Format("Stopwatch total: {0} ms", m_Stopwatch.ElapsedMilliseconds));
                else
                    UnityEngine.Debug.LogFormat(format, m_Stopwatch.ElapsedMilliseconds);
            }
        }
    }