
using UnityEngine;
using UnityEngine.UI;

namespace SFramework
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PolygonClickRangeImage : Image
    {
        private PolygonCollider2D _polygonCollider2D;


        private PolygonCollider2D PolygonCollider2D
        {
            get
            {
                if (_polygonCollider2D == null)
                {
                    _polygonCollider2D = GetComponent<PolygonCollider2D>();
                }

                return _polygonCollider2D;
            }
        }

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            Vector3 point;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, eventCamera, out point);
            return PolygonCollider2D.OverlapPoint(point);
        }
    }
}