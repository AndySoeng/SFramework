using UnityEngine;


namespace Ex
{
    public static class ExColor
    {
        /// <summary>
        /// 随机获取一个颜色
        /// </summary>
        /// <returns></returns>
        public static Color RandomColor()
        {
            float r = Random.Range(0f, 1f);
            float g = Random.Range(0f, 1f);
            float b = Random.Range(0f, 1f);
            Color color = new Color(r, g, b);
            return color;
        }
    }
}
