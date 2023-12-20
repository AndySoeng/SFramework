using UnityEngine;

namespace WebGLExpInterface.DTO
{
    /// <summary>
    /// 江苏一鼎堂软件科技有限公司接口2.0
    /// </summary>
    public class DTO_YDT
    {
        public class AccessTokenInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string  msg { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string access_token { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long create_time { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string create_time_display { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long expires_time { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string expires_time_display { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string un { get; set; }

            /// <summary>
            /// 用户姓名
            /// </summary>
            public string dis { get; set; }
        }

        /// <summary>
        /// 与原版ILab实体存在部分变量类型不同，故重新声明
        /// </summary>
        public class DTO_YDTILab
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
            public int appid;

            /// <summary>
            /// 实验平台实验记录ID：平台唯一且由大小写字母、数字、“_”组成
            /// </summary>
            public string originId;

            /// <summary>
            /// 实验步骤记录：详见《实验步骤记录》说明
            /// </summary>
            public DTO_ILab.Step[] steps;


            public DTO_YDTILab(int score, long startTime, long endTime, string username = "", string title = "", int status = 1, int appid = 0, string originId = "")
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

                DTO_ILab.Step step = new DTO_ILab.Step(nowStepLength + 1, title, startTime, endTime, maxScore, score);
                DTO_ILab.Step[] newSteps = new DTO_ILab.Step[nowStepLength + 1];
                for (int i = 0; i < nowStepLength; i++)
                {
                    newSteps[i] = steps[i];
                }

                newSteps[nowStepLength] = step;
                steps = newSteps;
            }

      
        }
        
        public class DataUploadRespon
        {
            public int code { get; set; }
            public string id { get; set; }
            public string msg { get; set; }
        }
    }
}