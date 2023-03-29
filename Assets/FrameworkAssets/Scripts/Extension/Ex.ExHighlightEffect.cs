using HighlightPlus;
using UnityEngine;


namespace Ex
{
    public static class ExHighlightEffect 
    {
        public static HighlightEffect HighlightEffectOverlayColorHDR(this HighlightEffect highlightEffect,
            float multiple)
        {
            highlightEffect.overlayColor = new Color(highlightEffect.overlayColor.r * multiple,
                highlightEffect.overlayColor.g * multiple, highlightEffect.overlayColor.b * multiple);
            return highlightEffect;
        }
    }
}
