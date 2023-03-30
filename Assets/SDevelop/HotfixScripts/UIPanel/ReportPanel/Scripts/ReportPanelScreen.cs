using Ex;
using LitJson;

namespace SFramework.UI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using PathologicalGames;
    using UnityEngine;
    using UnityEngine.UI;
    using SFramework;
    using DG.Tweening;

    public class ReportPanelScreenParam : UIOpenScreenParameterBase
    {
    }

    public class ReportPanelScreen : UIScreenBase
    {
        ReportPanelCtrl mCtrl;
        ReportPanelScreenParam mParam;

        protected override void OnLoadSuccess()
        {
            base.OnLoadSuccess();
            mCtrl = mCtrlBase as ReportPanelCtrl;

            //初始化实验报告
            InitReport();
            InitRepotItems();
            mCtrl.panel.DOFade(1, 0.5f);
        }

        #region 初始化实验报告

        public static bool ReportIsCommit = false;
        public static int StartsIndex = -1;
        public static string Conclusion = string.Empty;

        private void InitReport()
        {
            mCtrl.txt_Date.text = DateTime.Now.ToString("yyyy-MM-dd");
            mCtrl.btn_Close.onClick.AddListener(() =>
            {
                if (!ReportIsCommit)
                    NotificationsPanelScreen.ShowNotifications("实验报告提示", "完成试验后不要忘记提交实验报告哦~");
                SUIManager.Ins.CloseUI<ReportPanelScreen>();
            });

            mCtrl.input_Conclusion.text = Conclusion;
            mCtrl.input_Conclusion.onEndEdit.AddListener((arg0) =>
            {
                if (arg0.Length > 200)
                    NotificationsPanelScreen.ShowNotifications("实验报告提示", "心得体会不可超过200字,请进行修改！");
                Conclusion = arg0;
            });


            mCtrl.StarRating.selectorEvent.AddListener((index) => { StartsIndex = index; });
            if (StartsIndex != -1)
            {
                mCtrl.StarRating.defaultIndex = StartsIndex;
            }

            StartsIndex = mCtrl.StarRating.defaultIndex;
            mCtrl.btn_ReportCommit.onClick.AddListener(ReportCommit);


            if (ReportIsCommit)
            {
                Button[] btns = mCtrl.StarRating.GetComponentsInChildren<Button>();
                for (int i = 0; i < btns.Length; i++)
                {
                    btns[i].interactable = false;
                }

                mCtrl.btn_ReportCommit.interactable = false;
            }
        }

        public List<ReportDataItem> ReportDataItems = new List<ReportDataItem>();

        public void InitRepotItems()
        {
            SpawnPool spawnPool = PoolManager.Pools["Assignment module"];
            Transform ReportItem = spawnPool.prefabs["ReportItem"];

            //--------------------------------------- 根据数据生成实验得分项 --------------------------------------


            int sum = 0;
            foreach (var v in WebglExpData.ExpMoudleScore)
            {
                ReportDataItems.Add(spawnPool.Spawn(ReportItem, mCtrl.ReportList).ExUIResetZ()
                    .GetComponent<ReportDataItem>()
                    .Assignment(v.Key.ToString(), v.Value.ToString(), Ex.ExTime.GetTime(1000), "备注"));
                sum += v.Value;
            }

            mCtrl.txt_TotalScore.text = sum.ToString();
            //---------------------------------------------------------------------------------------------------
        }

        #endregion

        #region 实验报告提交

        [DllImport("__Internal")]
        private static extern string LoadParams();

        protected void ReportCommit()
        {
            if (Conclusion.Length < 200)
                ModalWindowPanelScreen.OpenModalWindowNoTabs("实验报告提示", "实验报告提交后不可以进行任何操作，是否确认提交？", true,
                    () => { ReportCommitting(mCtrl); });
            else
            {
                NotificationsPanelScreen.ShowNotifications("实验报告提示", "心得体会不可超过200字！");
            }
        }


        public static void ReportCommitting(MonoBehaviour monoBehaviour)
        {
#if UNITY_EDITOR
            string[] userInfos = new string[]
            {
                "评审专家",
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzY2hvb2wiOiLmtZnmsZ_kvKDlqpLlrabpmaIiLCJyb2xlSWQiOjMsImxvZ2luTmFtZSI6IjBlYTVlYzQ5LWYzNmQtNDMzNi04NGI0LTJmMzlmZjJiZDVmYyIsIm1pbGxUaW1lIjoxNjU1NDUyNjI3MjIxLCJpZCI6MTY5LCJleHAiOjE2NTU1MzkwMjcyMjEsInVzZXJuYW1lIjoi6K-E5a6h5LiT5a62In0.cDaROTlpOpjWNxwosgso9DZtwc6fxgmE2tlb3zwDNa0",
                "",
                "http://82.156.232.217:8910/zhjj/unity/addu3d"
            };
            // ExpData.ExpMoudleScore = new Dictionary<ExpMoudle, int>()
            // {
            //     {ExpMoudle.实验基础, 10}, //10
            //     {ExpMoudle.知识考核, UnityEngine.Random.Range(0, 11)}, //10
            //     {ExpMoudle.基础病认知模块, 10}, //10
            //     {ExpMoudle.基础病认知模块考核, UnityEngine.Random.Range(0, 11)}, //10
            //     {ExpMoudle.情景模拟模块, 10}, //10
            //     {ExpMoudle.情景模拟模块考核, UnityEngine.Random.Range(0, 11)},
            //     {ExpMoudle.数据处理与分析模块, 10}, //10
            //     {ExpMoudle.数据处理与分析模块考核, UnityEngine.Random.Range(0, 11)}, //10
            //     {ExpMoudle.预警模块, 10}, //10
            //     {ExpMoudle.预警模块考核, UnityEngine.Random.Range(0, 11)}, //10
            // };
            // ExpData.ExpMoudleFineshed = new Dictionary<ExpMoudle, bool>()
            // {
            //     {ExpMoudle.实验基础, true},
            //     {ExpMoudle.知识考核, true},
            //     {ExpMoudle.基础病认知模块, true},
            //     {ExpMoudle.基础病认知模块考核, true},
            //     {ExpMoudle.情景模拟模块, true},
            //     {ExpMoudle.情景模拟模块考核, true},
            //     {ExpMoudle.数据处理与分析模块, true},
            //     {ExpMoudle.数据处理与分析模块考核, true},
            //     {ExpMoudle.预警模块, true},
            //     {ExpMoudle.预警模块考核, true},
            // };
#else
        string[] userInfos = LoadParams().Split('~');
#endif


            bool finshedAll = true;
            foreach (var v in WebglExpData.ExpMoudleFineshed)
            {
                if (v.Value == false)
                    finshedAll = false;
            }

            if (finshedAll == false) //判断是否已经完成所有实验
            {
                NotificationsPanelScreen.ShowNotifications("提示", "当前未完成所有实验,请完成后进行提交。");
            }
            else if (string.IsNullOrEmpty(userInfos[3]) || string.IsNullOrEmpty(userInfos[1]))
            {
                NotificationsPanelScreen.ShowNotifications("提示", "未获取到成绩接口或token，请联系管理员。");
            }
            else
            {
                List<string[]> scoreList = new List<string[]>();

                DateTime EnterTime = WebglExpData.firstEnterTime;
                DateTime OverTime = EnterTime;

                int sumScore = 0;
                string[] scoreItem = null;
                foreach (var v in WebglExpData.ExpMoudleScore)
                {
                    OverTime = OverTime.AddSeconds(UnityEngine.Random.Range(0, 15));
                    scoreItem = new string[5];
                    scoreItem[0] = v.Key.ToString();
                    scoreItem[1] = OverTime.GetTimeStamp(false).ToString();
                    OverTime = OverTime.AddSeconds(UnityEngine.Random.Range(15, 45)).AddMinutes(UnityEngine.Random.Range(1, 2));
                    scoreItem[2] = OverTime.GetTimeStamp(false).ToString();
                    ;
                    scoreItem[3] = "10";
                    scoreItem[4] = v.Value.ToString();
                    scoreList.Add(scoreItem);

                    sumScore += v.Value;
                }

                OverTime = OverTime.AddSeconds(UnityEngine.Random.Range(5, 10));
                scoreItem = new string[5];
                scoreItem[0] = "总计";
                scoreItem[1] = EnterTime.GetTimeStamp(false).ToString();
                scoreItem[2] = OverTime.GetTimeStamp(false).ToString();
                scoreItem[3] = "100";
                scoreItem[4] = sumScore.ToString();
                scoreList.Add(scoreItem);


                //可以提交后打开遮挡
                SUIManager.Ins.OpenUI<HttpRequestPanelScreen>(new HttpRequestPanelScreenParam()
                    { content = "提交实验报告提交中……" });

                monoBehaviour.StartCoroutine(WebGLExpInterface.ExpInterfaceBase.WebRequest(WebGLExpInterface.UnityWebRequestType.POST, userInfos[3],
                    JsonMapper.ToJson(scoreList), false, false,
                    () =>
                    {
                        SUIManager.Ins.CloseUI<HttpRequestPanelScreen>();
                        ModalWindowPanelScreen.OpenModalWindowNoTabs("提示", "实验报告提提交失败，请重新提交。", true, null, false);
                    }, (str) =>
                    {
                        //提交成功的回调
                        SUIManager.Ins.CloseUI<HttpRequestPanelScreen>();
                        SUIManager.Ins.OpenUI<EndPanelScreen>(new EndPanelScreenParam()
                            { content = "实验报告已提交，实验结束。", isTransparent = false });
                    }, new[] { "Authorization" }, new[] { "Bearer " + userInfos[1] }));
            }
        }

        #endregion
    }
}