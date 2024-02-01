using System;
using Ex;
using UnityEngine;

namespace WebGLExpInterface.DTO
{
    public class DTO_Simtop1
    {
        /// <summary>
        /// 标识码
        /// </summary>
        public string serialNumber;
        /// <summary>
        /// 密钥
        /// </summary>
        public string salt;
        /// <summary>
        /// 实验成绩：0 ~100，百分制
        /// </summary>
        public int TotalScore;
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
        /// 实验结论
        /// </summary>
        public string conclusion;
        
        /// <summary>
        /// 实验状态：1 - 完成；2 - 未完成
        /// </summary>
        public int status;
        
        /// <summary>
        /// 实验步骤记录：详见《实验步骤记录》说明
        /// </summary>
        public DTO_ILab.Step[] stepList;




        public DTO_Simtop1(int score, long startTime, long endTime, int status = 1,string conclusion="")
        {
            this.TotalScore = score;
            this.startTime = startTime;
            this.endTime = endTime;
            this.timeUsed = (int)((endTime - startTime) / 1000);
            this.status = status;
            this.conclusion = conclusion;
        }

        

        public void AddStep(string title, long startTime, long endTime, int maxScore, int score)
        {
            int nowStepLength = stepList == null ? 0 : stepList.Length;

            DTO_ILab.Step step = new DTO_ILab.Step(nowStepLength + 1, title, startTime, endTime, maxScore, score);
            DTO_ILab.Step[] newSteps = new DTO_ILab.Step[nowStepLength + 1];
            for (int i = 0; i < nowStepLength; i++)
            {
                newSteps[i] = stepList[i];
            }

            newSteps[nowStepLength] = step;
            stepList = newSteps;
        }
        
        
        public class DataUploadRespon
        {
            public int status { get; set; }
            public string data { get; set; }
        }
    }
    
    
}