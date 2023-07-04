namespace WebGLExpInterface.DTO
{
    public class DTO_SimtopPlatformization
    {
        public class CXSYData
        {
            public int status;
            public int submitId;
            public int virtualExperimentScore;

            public string[] keys;
            public string[] values;

            public CXSYData(int status, int submitId, int virtualExperimentScore, string[] keys, string[] values)
            {
                this.status = status;
                this.submitId = submitId;
                this.virtualExperimentScore = virtualExperimentScore;
                this.keys = keys;
                this.values = values;
            }

            public CXSYData()
            {
            }
        }

        public class CXSYDataReply
        {
            public int code;
            public string msg;
        }
    }
}