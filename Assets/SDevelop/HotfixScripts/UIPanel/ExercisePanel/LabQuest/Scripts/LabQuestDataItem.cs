

using Ex;

namespace SFramework.UI
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using SFramework;
    using PathologicalGames;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class LabQuestDataItem : SerializedMonoBehaviour
    {
        public Dictionary<LabQuestSprite, Sprite> topicSprites = new Dictionary<LabQuestSprite, Sprite>();

        /// <summary>
        /// 题干类型图片
        /// </summary>
        public Image topicTypeSprite;

        /// <summary>
        /// 题干内容
        /// </summary>
        public TMP_Text topicContent;

        /// <summary>
        /// 题干图片组
        /// </summary>
        public GameObject topicImageGroup;

        /// <summary>
        /// 题干图片
        /// </summary>
        public Image topicImage;

        /// <summary>
        /// 题干图片名称
        /// </summary>
        public TMP_Text topicImageIndex;

        /// <summary>
        /// 图片选项父物体
        /// </summary>
        public GameObject imageOptionsToggleGroup;

        /// <summary>
        /// 文字选项父物体
        /// </summary>
        public GameObject textOptionsToggleGroup;

        /// <summary>
        /// 解析父物体
        /// </summary>
        public GameObject analyzeGroup;

        /// <summary>
        /// 解析内容
        /// </summary>
        public TMP_Text analyzeContent;

        [ReadOnly] public List<Toggle> ChoiceToggle = new List<Toggle>();
        [ReadOnly] public GameObject toggleParent;
        [ReadOnly] public Question quest;

        public LabQuestDataItem Init(Question quest, int Index,bool showIndex)
        {
            //清理集合
            ChoiceToggle.Clear();
            //设置题干
            topicTypeSprite.sprite = topicSprites[quest.questType];
            topicContent.text = (showIndex?(Index + "."):string.Empty) + quest.topicContent;

            //设置题干图片
            if (quest.topicHaveSprite)
            {
                topicImageGroup.SetActive(true);
                topicImage.sprite = quest.topicSprite;
                topicImageIndex.text = quest.topicSpriteName;
            }
            else topicImageGroup.SetActive(false);

            SpawnPool spawnPool = PoolManager.Pools["LabQuest"];


            Transform OptionPrefab =
                quest.optionIsSprite ? spawnPool.prefabs["ImageOption"] : spawnPool.prefabs["TextOption"];
            //设置选项容器
            imageOptionsToggleGroup.SetActive(quest.optionIsSprite);
            textOptionsToggleGroup.SetActive(!quest.optionIsSprite);
            toggleParent = quest.optionIsSprite ? imageOptionsToggleGroup : textOptionsToggleGroup;
            toggleParent.GetComponent<ToggleGroup>().allowSwitchOff = true;

            //设置选项
            int choicesCount = quest.optionIsSprite ? quest.spriteChoices.Count : quest.txtChoices.Count;
            for (int i = 0; i < choicesCount; i++)
            {
                ChoiceToggle.Add(spawnPool.Spawn(OptionPrefab, toggleParent.transform).ExUIResetZ().GetComponent<LabQuestDataOption>()
                    .InitOption(quest.optionIsSprite ? (Choice)quest.spriteChoices[i] : (Choice)quest.txtChoices[i]));
                ChoiceToggle[i].interactable = true;
                ChoiceToggle[i].isOn = false;
            }

            //处理题目类型逻辑(多选不需处理)
            if (quest.questType == LabQuestSprite.Single || quest.questType == LabQuestSprite.Judgment)
            {
                for (int i = 0; i < ChoiceToggle.Count; i++)
                {
                    int indexI = i;
                    ChoiceToggle[indexI].onValueChanged.AddListener(SingleAndJudgmentChoiceToggleOnValueChanged);
                }
            }

            //设置解析隐藏
            analyzeGroup.SetActive(false);

            //保存试题引用
            this.quest = quest;

            return this;
        }
        

        public void SingleAndJudgmentChoiceToggleOnValueChanged(bool isTrue)
        {
            if (isTrue)
            {
                toggleParent.GetComponent<ToggleGroup>().allowSwitchOff = false;
                for (int i = 0; i < ChoiceToggle.Count; i++)
                {
                    int indexI = i;
                    ChoiceToggle[indexI].group = toggleParent.GetComponent<ToggleGroup>();
                    ChoiceToggle[indexI].onValueChanged.RemoveListener(SingleAndJudgmentChoiceToggleOnValueChanged);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canModify">是否能够继续修改</param>
        /// <returns></returns>
        public bool Sumbit(bool canModify = false)
        {
            bool isRight = true;
            for (int i = 0; i < ChoiceToggle.Count; i++)
            {
                //判断是否选择了正确选项
                if (quest.CorrectOption.Contains(ChoiceToggle[i].GetComponent<LabQuestDataOption>().choiceIndexStr) && ChoiceToggle[i].isOn) //是正确答案并且选择了
                {
                }
                else if (quest.CorrectOption.Contains(ChoiceToggle[i].GetComponent<LabQuestDataOption>().choiceIndexStr) && !ChoiceToggle[i].isOn) //是正确答案并且没选
                    isRight = false;
                else if (!quest.CorrectOption.Contains(ChoiceToggle[i].GetComponent<LabQuestDataOption>().choiceIndexStr) && ChoiceToggle[i].isOn) //不是正确答案并且选择了
                    isRight = false;
                else if (quest.CorrectOption.Contains(ChoiceToggle[i].GetComponent<LabQuestDataOption>().choiceIndexStr) && !ChoiceToggle[i].isOn) //不是正确答案并且没选
                {
                }
            }

            //如果能够继续修改就直接返回对错，否则直接禁用所有交互
            if (canModify)
                return isRight;
            
            
            
            for (int i = 0; i < ChoiceToggle.Count; i++)
            {
                //清除Toggle事件和交互
                ChoiceToggle[i].onValueChanged.RemoveListener(SingleAndJudgmentChoiceToggleOnValueChanged);
                ChoiceToggle[i].interactable = false;
            }
            
            StringBuilder resultNotice = new StringBuilder();
            if (isRight)
                resultNotice.Append("<color=green>作答正确！</color>");
            else
            {
                resultNotice.Append("<color=red>作答错误！正确答案为：");
                for (int i = 0; i < quest.CorrectOption.Count; i++)
                {
                    resultNotice.Append(quest.CorrectOption[i]);
                }

                resultNotice.Append("。</color>");
            }

            resultNotice.Append("解析：");
            resultNotice.Append(String.IsNullOrEmpty(quest.analyzeContent) ? "无。" : quest.analyzeContent);
            analyzeContent.text = resultNotice.ToString();
            analyzeGroup.SetActive(true);
            return isRight;
        }

        /// <summary>
        /// 记录用户所选择选项
        /// </summary>
        public List<ChoiceIndexStr> SaveLabQuestChoice()
        {
            List<ChoiceIndexStr> choiceIndexs = new List<ChoiceIndexStr>();
            for (int i = 0; i < ChoiceToggle.Count; i++)
            {
                if (ChoiceToggle[i].isOn)
                {
                    choiceIndexs.Add(ChoiceToggle[i].GetComponent<LabQuestDataOption>().choiceIndexStr);
                }
            }

            return choiceIndexs;
        }

        /// <summary>
        /// 加载用户所选择选项
        /// </summary>
        public void ReloadLabQuestChoice(List<ChoiceIndexStr> choiceIndexs)
        {
            for (int i = 0; i < ChoiceToggle.Count; i++)
            {
                if (choiceIndexs.Contains(ChoiceToggle[i].GetComponent<LabQuestDataOption>().choiceIndexStr))
                {
                    ChoiceToggle[i].isOn = true;
                }
            }
        }

        /// <summary>
        /// 检查用户是否进行选择了试题
        /// </summary>
        /// <returns>true为当前试题已经进行了选择，false说明当前试题未进行选择</returns>
        public bool CheckUserIsCheck()
        {
            for (int i = 0; i < ChoiceToggle.Count; i++)
            {
                if (ChoiceToggle[i].isOn)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public enum LabQuestSprite
    {
        Single, //单选
        Multiple, //多选
        Judgment, //判断
    }
}