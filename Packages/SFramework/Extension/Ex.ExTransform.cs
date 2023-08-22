using System.Reflection;
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
        
        
        public static Vector3 GetInspectorRotationValue(Transform transform)
        {
            System.Type transformType = transform.GetType();
            PropertyInfo m_propertyInfo_rotationOrder = transformType.GetProperty("rotationOrder", BindingFlags.Instance | BindingFlags.NonPublic);
            object m_OldRotationOrder = m_propertyInfo_rotationOrder.GetValue(transform, null);
            MethodInfo m_methodInfo_GetLocalEulerAngles = transformType.GetMethod("GetLocalEulerAngles",BindingFlags.Instance | BindingFlags.NonPublic);
            object value = m_methodInfo_GetLocalEulerAngles.Invoke(transform, new object[] {m_OldRotationOrder});
            return (Vector3) value;
        }
         
        
        
        
        /// <summary>
        /// 返回物体正前方的位置
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 Forward(this Transform trans, float distance)
        {
            return trans.position+ trans.TransformDirection(Vector3.forward) * distance;
        }
        
        
        
        /// <summary>
        /// 距离trans正方向dis距离的一个点，绕一定y\z角度旋转后的位置
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="dis"></param>
        /// <param name="angleUp"></param>
        /// <param name="angleRight"></param>
        /// <returns></returns>
        public  static Vector3  GenFovPos(this Transform trans, float dis, float angleUp, float angleRight)
        {
            Vector3 originalPoint = trans.position + trans.forward * dis;
            // 计算绕物体的Y轴60度的旋转
            Quaternion rotationAroundLocalY = Quaternion.AngleAxis(angleUp, trans.up);

            // 计算绕物体的Z轴30度的旋转
            Quaternion rotationAroundLocalX = Quaternion.AngleAxis(angleRight, trans.right);

            // 应用旋转
            Vector3 pos= trans.position + rotationAroundLocalY * rotationAroundLocalX * (originalPoint - trans.position);
            // GameObject a = new GameObject();
            // a.transform.SetParent(trans);
            // a.transform.position = pos;

            return pos;
        }
    }
}
