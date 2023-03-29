namespace SFramework.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(order = 1, menuName = "ELConfigs/LabQuestData")]
    public class LabQuestData : SerializedScriptableObject
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<Question> questions;
    }

    [Serializable]
    public class Question
    {
        [Title("TopicContentSetting")] public LabQuestSprite questType;
        [Multiline(5)] [HideLabel] public string topicContent;

        [Title("TopicSpriteSetting")] public bool topicHaveSprite;

        [ShowIf("topicHaveSprite")] [HideLabel] [PreviewField(150, ObjectFieldAlignment.Center)]
        public Sprite topicSprite;

        [ShowIf("topicHaveSprite")] public string topicSpriteName;

        [Title("OptionSetting")] public bool optionIsSprite;

        [HideIf("optionIsSprite")] public List<TxtChoice> txtChoices;

        [ShowIf("optionIsSprite")] public List<SpriteChoice> spriteChoices;

        public List<ChoiceIndexStr> CorrectOption;

        [Title("AnalyzeContentSetting")] [Multiline(3)] [HideLabel]
        public string analyzeContent;
    }

    public class Choice
    {
    }

    [Serializable]
    public class SpriteChoice : Choice
    {
        public ChoiceIndexStr option;
        public Sprite choiceSprite;
    }

    [Serializable]
    public class TxtChoice : Choice
    {
        public ChoiceIndexStr option;
        public string choiceContent;
    }

    public enum ChoiceIndexStr
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z
    };
}