
namespace WebGLExpInterface.DTO
{
    public class DTO_ChenXuan
    {
        public class StatusInfo
        {
            public int code;
            public string message;
            public int data;
        }

        public class DTO_AddExperiment
        {
            /// <summary>
            /// 客户名
            /// </summary>
            public string customName;

            /// <summary>
            /// 用户账号类型
            /// </summary>
            public string accountType;

            /// <summary>
            /// 用户账号
            /// </summary>
            public string accountNumber;

            /// <summary>
            /// 用户名
            /// </summary>
            public string userName;

            /// <summary>
            /// token
            /// </summary>
            public string accessToken;

            /// <summary>
            /// 实验信息json
            /// </summary>
            public string contextJson;

            /// <summary>
            /// 实验平台的URL
            /// </summary>
            public string remoteUrl;

            /// <summary>
            /// 类型（1：普通学生实验保存  2：评审实验保存）
            /// </summary>
            public string type;

            public DTO_AddExperiment(string accessToken, string contextJson)
            {
                this.accessToken = accessToken;
                this.contextJson = contextJson;
            }
        }
    }
}