using System.Text;

namespace Ex
{
    public static class ExStringBuilder
    {
        private static StringBuilder m_StringBuilder = new StringBuilder();

        public static StringBuilder ClearAndAppend(string str)
        {
            m_StringBuilder.Clear();
            m_StringBuilder.Append(str);
            return m_StringBuilder;
        }
    }
}