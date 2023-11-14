using UnityEngine;
using UnityEngine.UI;


namespace Ex
{
    public static class ExUI_ScrollRect
    {
        public static Vector2 Nevigate(ScrollRect scrollRect, RectTransform viewport, RectTransform content, RectTransform item)
        {
            //先刷新一下布局，因为动态生成的Item如果立即调用Nevigate会造成NormalizedPos计算错误
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            // InverseTransformPoint: Transforms position from world space to local space, 和TransformPoint左右相反
            // 这步的意义是把 item、viewport的localPosition转换到同一个父节点下，才能计算出需要移动的差值
            // 看图1
            Vector3 itemCurrentLocalPostion = scrollRect.GetComponent<RectTransform>().InverseTransformPoint(ConvertLocalPosToWorldPos(item));
            Vector3 itemTargetLocalPos = scrollRect.GetComponent<RectTransform>().InverseTransformPoint(ConvertLocalPosToWorldPos(viewport));
            // 计算需要移动的距离
            Vector3 diff = itemTargetLocalPos - itemCurrentLocalPostion;

            // 当第一个item处于viewport最上时 verticalNormalizedPosition = 0，处于最左时 horizontalNormalizedPosition = 0，看图2

            // 计算需要移动的距离占的比，即需要移动的距离占可移动长度的百分比，看图3
            // 如果你的 viewport 中只能显示一个item的话，这样就行了，但是超过了1个的话需要计算偏移：
            // 以数值方向为例，当verticalNormalizedPosition = 0，第一个 item 距离 viewport的中心位置其实有一段距离的，
            // 所以要减去这段距离，diff.y - offset，再计算 newNormalizedPosition，看图4
            var newNormalizedPosition = new Vector2(
                diff.x / (content.GetComponent<RectTransform>().rect.width - viewport.rect.width),
                diff.y / (content.GetComponent<RectTransform>().rect.height - viewport.rect.height)
            );
            // 当时 normalizedPosition - 需要移动的占比newNormalizedPosition，得到最终的位置normalizedPosition
            newNormalizedPosition = scrollRect.GetComponent<ScrollRect>().normalizedPosition - newNormalizedPosition;

            newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
            newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
            // 也可以只设置水平方向 horizontalNormalizedPosition，或竖直方向 verticalNormalizedPosition
            // 设置 normalizedPosition 等于同时设置 horizontalNormalizedPosition， verticalNormalizedPosition
            // normalizedPosition.x == horizontalNormalizedPosition
            // normalizedPosition.y == verticalNormalizedPosition
            // normalizedPosition == new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition)
            //scrollRect.GetComponent<ScrollRect>().normalizedPosition = newNormalizedPosition;
            return newNormalizedPosition;

            // 如果有分类功能，比如点击type1,scrollview中显示type1中的数据,点击type2，scrollview中显示type2中的数据，需要在给 
            // normalizedPosition 赋值前先赋值 (0, 0)，否则最后一行代码获取的 normalizedPosition 还是上一类型 type 中的 
            // normalizedPosition，导致出bug
        }

        // 这个方法的作用是消除Pivot数值的影响，Pivot不是(0.5，0.5)时，最后所计算出来的结果会有误差
        // 如果是(0.5, 0.5)的话可以直接返回 target.parent.TransformPoint(localPosition)
        private static Vector3 ConvertLocalPosToWorldPos(RectTransform target)
        {
            var pivotOffset = new Vector3(
                (0.5f - target.pivot.x) * target.rect.size.x,
                (0.5f - target.pivot.y) * target.rect.size.y,
                0f);

            var localPosition = target.localPosition + pivotOffset;
            // TransformPoint: Transforms position from local space to world space
            return target.parent.TransformPoint(localPosition);
        }
    }
}