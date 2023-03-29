

namespace SFramework.UI
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    [CreateAssetMenu(order = 1, menuName = "ELConfigs/DetailItemContent")]
    public class DetailItemContent : SerializedScriptableObject
    {
        public string title;
        public PromptClassification classify;

        [ShowIf("classify", PromptClassification.文字)] [TextArea(3, 10)]
        public string txtContent;

        [ShowIf("classify", PromptClassification.图片)]
        public Sprite spriteContent;

        [ShowIf("classify", PromptClassification.分页)]
        public List<PageContent> pageContents = new List<PageContent>();
    }

    public enum PromptClassification
    {
        文字,
        图片,
        分页,
    }

    /// <summary>
    /// 分页内容
    /// </summary>
    [Serializable]
    public class PageContent
    {
        public string title;
        public bool isSprite = false;
        [HideIf("isSprite")] [TextArea(3, 10)] public string txtContent;
        [ShowIf("isSprite")] public Sprite spriteContent;
    }
}