using UnityEngine;


namespace Ex
{
    public static class ExTransform
    {
        /// <summary>
        /// 对象池加载重置Z轴
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static Transform ExUIResetZ(this Transform trans)
        {
            Vector3 v3 = trans.GetComponent<RectTransform>().localPosition;
            v3.z = 0;
            trans.GetComponent<RectTransform>().localPosition = v3;
            return trans;
        }


        public static Transform ExSetAsFirstSibling(this Transform trans)
        {
            trans.SetAsFirstSibling();
            return trans;
        }

        public static Transform ExSetAsLastSibling(this Transform trans)
        {
            trans.SetAsLastSibling();
            return trans;
        }

        /// <summary>
        /// 对象池加载重置XYZ轴
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static Transform ExUIResetXYZ(this Transform trans, Vector3 localPosition)
        {
            trans.GetComponent<RectTransform>().localPosition = localPosition;
            trans.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            return trans;
        }

        public static Transform ExUIResetSizeDelta(this Transform trans, Vector2 sizeDelta)
        {
            trans.GetComponent<RectTransform>().sizeDelta = sizeDelta;
            return trans;
        }
    }
}
