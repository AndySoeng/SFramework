using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.UI
{
    public class RectMask : Graphic, ICanvasRaycastFilter
    {
        public RectTransform _target;
        private Vector3 _targetBoundsMin;
        private Vector3 _targetBoundsMax;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) return;
#endif
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(this.rectTransform, this._target);
            this._targetBoundsMin = bounds.min;
            this._targetBoundsMax = bounds.max;

            if (this._targetBoundsMin == Vector3.zero && this._targetBoundsMax == Vector3.zero)
            {
                base.OnPopulateMesh(vh);
                return;
            }

            vh.Clear();

            Vector2 pivot = this.rectTransform.pivot;
            Rect rect = this.rectTransform.rect;
            float outerLeftBottomX = -pivot.x * rect.width;
            float outerLeftBottomY = -pivot.y * rect.height;
            float outerRightTopX = (1 - pivot.x) * rect.width;
            float outerRightTopY = (1 - pivot.y) * rect.height;

            // 准备顶点数据
            UIVertex vert = UIVertex.simpleVert;
            // 填充顶点颜色
            vert.color = this.color;

            // 计算遮罩区域顶点位置
            // 0 outer LeftTop
            vert.position = new Vector3(outerLeftBottomX, outerRightTopY);
            vh.AddVert(vert);
            // 1 outer RightTop
            vert.position = new Vector3(outerRightTopX, outerRightTopY);
            vh.AddVert(vert);
            // 2 outer RightBottom
            vert.position = new Vector3(outerRightTopX, outerLeftBottomY);
            vh.AddVert(vert);
            // 3 outer LeftBottom
            vert.position = new Vector3(outerLeftBottomX, outerLeftBottomY);
            vh.AddVert(vert);

            // 计算镂空区域顶点位置
            // 4 inner LeftTop
            vert.position = new Vector3(_targetBoundsMin.x, _targetBoundsMax.y);
            vh.AddVert(vert);
            // 5 inner RightTop
            vert.position = new Vector3(_targetBoundsMax.x, _targetBoundsMax.y);
            vh.AddVert(vert);
            // 6 inner RightBottom
            vert.position = new Vector3(_targetBoundsMax.x, _targetBoundsMin.y);
            vh.AddVert(vert);
            // 7 inner LeftBottom
            vert.position = new Vector3(_targetBoundsMin.x, _targetBoundsMin.y);
            vh.AddVert(vert);

            // 向缓冲区中添加三角形
            vh.AddTriangle(4, 0, 1);
            vh.AddTriangle(4, 1, 5);
            vh.AddTriangle(5, 1, 2);
            vh.AddTriangle(5, 2, 6);
            vh.AddTriangle(6, 2, 3);
            vh.AddTriangle(6, 3, 7);
            vh.AddTriangle(7, 3, 0);
            vh.AddTriangle(7, 0, 4);
        }


        /// <summary>
        /// 镂空区域内事件透传
        /// 给定一个点和一个摄像机，判断射线投射是否有效，通常用于自定义 UI 元素的交互，通过继承 ICanvasRaycastFilter 接口并实现该方法来自定义 UI 元素射线检测效果
        /// screenPoint 参数是一个屏幕坐标系下的二维向量，表示射线检测的位置，eventCamera 参数是一个 Camera 类型的对象，表示射线检测所使用的相机。该方法返回一个 bool 类型的值，表示 UI 元素是否可以被射线检测到
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="eventCamera"></param>
        /// <returns></returns>
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // 透传镂空区域事件
            return !RectTransformUtility.RectangleContainsScreenPoint(_target, sp, eventCamera);
        }


        public void Show(RectTransform target)
        {
            _target = target;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _target = null;
            gameObject.SetActive(false);
        }
    }
}