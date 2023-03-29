using DG.Tweening;
using Ex;
using Michsky.UI.ModernUIPack;
using TMPro;

namespace SFramework.UI
{
    using System.Collections.Generic;
    using SFramework;
    using PathologicalGames;
    using UnityEngine;
    using System;

    public class ExercisePanelScreenParam : UIOpenScreenParameterBase
    {
        public string exerciseName;
        public Action<string,int, int> commitCallBack;
        public bool cacheResult = true;
        public bool canRefresh = true;
        public bool canClose = true;
        /// <summary>
        /// 严格模式下，必须全部答对所有答案，才会提交
        /// </summary>
        public bool strictMode = false; 
        public Action closeCallBack;
        public bool showIndex = true;
        public string panelTitle = "";
        public string commitBtnTxt = "提交实验习题";
    }

    public class ExercisePanelScreen : UIScreenBase
    {
        ExercisePanelCtrl mCtrl;
        ExercisePanelScreenParam mParam;
        
        private LabQuestData currentLabQuestData = null;

        protected override void OnLoadSuccess()
        {
            base.OnLoadSuccess();
            mCtrl = mCtrlBase as ExercisePanelCtrl;
            mParam = mOpenParam as ExercisePanelScreenParam;
            currentLabQuestData = Resources.Load<LabQuestData>("Configs/Exercise/" + mParam.exerciseName);
            //初始化实验习题
            InitQuestion();
            mCtrl.panel.DOFade(1, 0.5f);
        }

        public static void ShowExercise(string exerciseName, Action<string, int,int> commitCallBack, bool cacheResult = true, bool canRefresh = true, bool canClose = true,
            bool strictMode = false,Action closeCallBack=null,bool showIndex=true,string panelTitle = "",string commitBtnTxt="")
        {
             SUIManager.Ins.OpenUI<ExercisePanelScreen>(new ExercisePanelScreenParam()
            {
                exerciseName = exerciseName,
                commitCallBack = commitCallBack,
                cacheResult = cacheResult,
                canRefresh = canRefresh,
                canClose = canClose,
                strictMode=strictMode,
                closeCallBack=closeCallBack,
                showIndex = showIndex,
                panelTitle = panelTitle,
                commitBtnTxt = commitBtnTxt,
            });
        }

        #region 实验习题

        private static Dictionary<string, bool> LabQuestIsCommit = new Dictionary<string, bool>();
        private static Dictionary<string, List<List<ChoiceIndexStr>>> UserChoice = new Dictionary<string, List<List<ChoiceIndexStr>>>();

        private void InitQuestion()
        {
            if (!string.IsNullOrEmpty(mParam.panelTitle))
            {
                mCtrl.go_Title.SetActive(true);
                mCtrl.go_Title.GetComponent<TMP_Text>().text = mParam.panelTitle;
            }
            if (mParam.cacheResult && !LabQuestIsCommit.ContainsKey(mParam.exerciseName))
                LabQuestIsCommit.Add(mParam.exerciseName, false);
            if (mParam.cacheResult && !UserChoice.ContainsKey(mParam.exerciseName))
                UserChoice.Add(mParam.exerciseName, new List<List<ChoiceIndexStr>>());
            InitLabQuest();
            InitLabQuestCommit();
            mCtrl.btn_LabQuestRefresh.gameObject.SetActive(mParam.canRefresh);
            mCtrl.btn_LabQuestRefresh.onClick.AddListener(LabQuestRefresh);
            mCtrl.btn_LabQuestClose.gameObject.SetActive(mParam.canClose);
            mCtrl.btn_LabQuestClose.onClick.AddListener(CloseExersise);
        }

        private void CloseExersise()
        {
            if (!mParam.cacheResult || LabQuestIsCommit[mParam.exerciseName])
            {
                SUIManager.Ins.CloseUI<ExercisePanelScreen>();
                mParam.closeCallBack?.Invoke();
            }
            else
                ModalWindowPanelScreen.OpenModalWindowNoTabs("系统提示", "未提交前退出不会保存已选择结果哦~", true, () => { SUIManager.Ins.CloseUI<ExercisePanelScreen>(); });
        }

        public List<LabQuestDataItem> labQuestDataItems = new List<LabQuestDataItem>();

        /// <summary>
        /// 初始化习题
        /// </summary>
        protected void InitLabQuest()
        {
            //清除labQuestDataItems引用
            labQuestDataItems.Clear();
            //加载试题开始生成
            List<Question> quests = currentLabQuestData.questions; //LabQuestData
            SpawnPool spawnPool = PoolManager.Pools["LabQuest"];
            Transform labQuestItem = spawnPool.prefabs["LabQuestItem"];
            for (int i = 0; i < quests.Count; i++)
            {
                labQuestDataItems.Add(spawnPool.Spawn(labQuestItem, mCtrl.trans_LabQuestParent).ExUIResetZ()
                    .GetComponent<LabQuestDataItem>().Init(quests[i], i + 1,mParam.showIndex));
            }
        }

        /// <summary>
        /// 根据习题是否提交进行习题初始化
        /// </summary>
        protected void InitLabQuestCommit()
        {
            if (!string.IsNullOrEmpty(mParam.commitBtnTxt))
            {
                mCtrl.btn_LabQuestCommit.GetComponent<ButtonManager>().buttonText = mParam.commitBtnTxt;
                mCtrl.btn_LabQuestCommit.GetComponent<ButtonManager>().UpdateUI();
            }
            if (LabQuestIsCommit.ContainsKey(mParam.exerciseName) && LabQuestIsCommit[mParam.exerciseName])
            {
                ReloadLabQuestChoice();
                mCtrl.btn_LabQuestCommit.interactable = false;
            }
            else
            {
                mCtrl.btn_LabQuestCommit.onClick.AddListener(VerificationLabQuest);
            }
        }

        /// <summary>
        /// 重新加载用户选项
        /// </summary>
        protected void ReloadLabQuestChoice()
        {
            for (int i = 0; i < labQuestDataItems.Count; i++)
            {
                labQuestDataItems[i].ReloadLabQuestChoice(UserChoice[mParam.exerciseName][i]);
                labQuestDataItems[i].Sumbit();
            }
        }


        /// <summary>
        /// 检查用户是否已经做完习题
        /// </summary>
        /// <returns>返回true为已经全部都进行选择，返回false说明有试题未进行选择</returns>
        protected bool CheckUserIsChoice()
        {
            for (int i = 0; i < labQuestDataItems.Count; i++)
            {
                if (!labQuestDataItems[i].CheckUserIsCheck())
                {
                    return false;
                }
            }

            return true;
        }


        protected void VerificationLabQuest()
        {
            if (CheckUserIsChoice())
            {
                ModalWindowPanelScreen.OpenModalWindowNoTabs("提示", "已作答完，是否确认？", true, CommitLabQuest,true);
            }
            else if (!CheckUserIsChoice()&&mParam.strictMode)
            {
                ModalWindowPanelScreen.OpenModalWindowNoTabs("提示", "作答未完成，请先完成作答。", true,null ,false);

            }
            else if (!CheckUserIsChoice()&&!mParam.strictMode)
            {
                ModalWindowPanelScreen.OpenModalWindowNoTabs("提示", "作答未完成，是否确认？", true, CommitLabQuest,true);
            }
        }

        /// <summary>
        /// 提交实验习题
        /// </summary>
        protected void CommitLabQuest()
        {
            int rightNum = 0;
            List<List<ChoiceIndexStr>> userChoices = new List<List<ChoiceIndexStr>>();
            for (int i = 0; i < labQuestDataItems.Count; i++)
            {
                if (labQuestDataItems[i].Sumbit(true))
                {
                    rightNum++;
                }

                userChoices.Add(labQuestDataItems[i].SaveLabQuestChoice());
            }


            if (mParam.strictMode)
            {
                if (rightNum != currentLabQuestData.questions.Count)
                {
                    NotificationsPanelScreen.ShowNotifications("提示", "当前习题需全答对才可提交,回答存在错误，请重新作答。");
                    return;
                }
            }

            for (int i = 0; i < labQuestDataItems.Count; i++)
            {
                labQuestDataItems[i].Sumbit();
            }

            mCtrl.btn_LabQuestCommit.onClick.RemoveAllListeners();
            mCtrl.btn_LabQuestCommit.interactable = false;

            if (mParam.cacheResult)
            {
                UserChoice[mParam.exerciseName].AddRange(userChoices);
                LabQuestIsCommit[mParam.exerciseName] = true;
            }

            mParam.commitCallBack?.Invoke(mParam.exerciseName,currentLabQuestData.questions.Count, rightNum);

            //如果没有关闭按钮，提交后应打开关闭按钮
            if (!mParam.canClose)
            {
                mCtrl.btn_LabQuestClose.gameObject.SetActive(true);
                mCtrl.go_CloseTip.SetActive(true);
            }
        }

        protected void LabQuestRefresh()
        {
            if (mParam.cacheResult)
                ModalWindowPanelScreen.OpenModalWindowNoTabs("提示",
                    LabQuestIsCommit[mParam.exerciseName] ? "重置后当前所选项将被重置并且已得分将清零，继续重置么？" : "重置后当前所选项将被重置，继续重置么？", true,
                    ResetAllLabQuest);
            else
                ModalWindowPanelScreen.OpenModalWindowNoTabs("提示",
                    "重置后当前所选项将被重置，继续重置么？", true,
                    ResetAllLabQuest);
        }

        protected void ResetAllLabQuest()
        {
            PoolManager.Pools["LabQuest"].DespawnAll();

            mCtrl.btn_LabQuestCommit.interactable = true;
            LabQuestIsCommit[mParam.exerciseName] = false;
            UserChoice[mParam.exerciseName].Clear();
            InitLabQuest();
            InitLabQuestCommit();
        }

        #endregion
    }
}