using UnityEngine;

/// <summary>
/// data_upload
/// </summary>
public class DTO_ILabDataUpload
{
    /// <summary>
    /// 实验空间用户账号
    /// </summary>
    public string username;

    /// <summary>
    /// 实验名称：用户学习的实验名称（100字以内）
    /// </summary>
    public string title;

    /// <summary>
    /// 实验状态：1 - 完成；2 - 未完成
    /// </summary>
    public int status;

    /// <summary>
    /// 实验成绩：0 ~100，百分制
    /// </summary>
    public int score;

    /// <summary>
    /// 实验开始时间：13位时间戳
    /// </summary>
    public long startTime;

    /// <summary>
    /// 实验结束时间：13位时间戳
    /// </summary>
    public long endTime;

    /// <summary>
    /// 实验用时：非零整数，单位秒
    /// </summary>
    public int timeUsed;

    /// <summary>
    /// 接入平台编号：由“实验空间”分配给实验教学项目的编号
    /// </summary>
    public string appid;

    /// <summary>
    /// 实验平台实验记录ID：平台唯一且由大小写字母、数字、“_”组成
    /// </summary>
    public string originId;

    /// <summary>
    /// 实验步骤记录：详见《实验步骤记录》说明
    /// </summary>
    public Step[] steps;


    public DTO_ILabDataUpload(int score, long startTime, long endTime, string username = "", string title = "", int status = 1, string appid = "", string originId = "")
    {
        this.username = username;
        this.title = string.IsNullOrEmpty(title) ? Application.productName : title;
        this.status = status;
        this.score = score;
        this.startTime = startTime;
        this.endTime = endTime;
        this.timeUsed = (int)((endTime - startTime) / 1000);
        this.appid = appid;
        this.originId = originId;
    }


    public void AddStep(string title, long startTime, long endTime, int maxScore, int score)
    {
        int nowStepLength = steps == null ? 0 : steps.Length;

        Step step = new Step(nowStepLength + 1, title, startTime, endTime,maxScore,score);
        Step[] newSteps = new Step[nowStepLength + 1];
        for (int i = 0; i < nowStepLength; i++)
        {
            newSteps[i] = steps[i];
        }

        newSteps[nowStepLength] = step;
        steps = newSteps;
    }

    public class Step
    {
        /// <summary>
        /// 实验步骤序号
        /// </summary>
        public int seq;

        /// <summary>
        /// 步骤名称：100字以内
        /// </summary>
        public string title;

        /// <summary>
        /// 实验步骤开始时间：13位时间戳
        /// </summary>
        public long startTime;

        /// <summary>
        /// 实验步骤结束时间：13位时间戳
        /// </summary>
        public long endTime;

        /// <summary>
        /// 实验步骤用时：非零整数，单位秒
        /// </summary>
        public int timeUsed;

        /// <summary>
        /// 实验步骤合理用时：单位秒
        /// </summary>
        public int expectTime;

        /// <summary>
        /// 实验步骤满分：0 ~100，百分制
        /// </summary>
        public int maxScore;

        /// <summary>
        /// 实验步骤得分：0 ~100，百分制
        /// </summary>
        public int score;

        /// <summary>
        /// 实验步骤操作次数
        /// </summary>
        public int repeatCount;

        /// <summary>
        /// 步骤评价：200字以内
        /// </summary>
        public string evaluation;

        /// <summary>
        /// 赋分模型：200字以内
        /// </summary>
        public string scoringModel;

        /// <summary>
        /// 备注：200字以内
        /// </summary>
        public string remarks;


        public Step(int seq, string title, long startTime, long endTime, int maxScore, int score, int expectTime = 180, int repeatCount = 1, string evaluation = "优",
            string scoringModel = "赋分模型", string remarks = "备注")
        {
            this.seq = seq;
            this.title = title;
            this.startTime = startTime;
            this.endTime = endTime;
            this.timeUsed = (int)((endTime - startTime) / 1000);
            this.expectTime = expectTime;
            this.maxScore = maxScore;
            this.score = score;
            this.repeatCount = repeatCount;
            this.evaluation = evaluation;
            this.scoringModel = scoringModel;
            this.remarks = remarks;
        }
    }
}