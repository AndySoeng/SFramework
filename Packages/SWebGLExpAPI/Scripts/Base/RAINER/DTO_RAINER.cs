using System.Collections.Generic;
using UnityEngine;

namespace WebGLExpInterface.DTO
{
    public class DTO_RAINER
    {
        
    public class UserInfo
    {
        public string status { get; set; }
        public string eId { get; set; }
        public string userId { get; set; }
        public string numberId { get; set; }
        public string name { get; set; }
        public string groupName { get; set; }
        public string host { get; set; }
        public string role { get; set; }

        public string statusMessage { get; set; }

        public override string ToString()
        {
            return status + "\t" + eId + "\t" + userId + "\t" + numberId + "\t" + name + "\t" + groupName + "\t" +
                   host + "\t" + role + "\t" + statusMessage;
        }
    }

//{"status":"000",
//"eId":"8a8091397217a91a01722705b0b80fb4",
//"userId":"ff8080817048d4830170624769800cde",
//"numberId":"20202020",
//"groupName":"无",
//"name":"333",
//"host":"http://zhang.xb.owvlab.net/virexp",
//"role":"student"}


    public class Token
    {
        public string token { get; set; }
    }


    public class SendExpScore
    {
        public string eid { get; set; }
        public string expScore { get; set; }
    }

    public class StatusInfo
    {
        // 000	成功
        // 101	数据库异常
        // 其他	系统错误
        public string status { get; set; }
        public string statusMessage { get; set; }


        public override string ToString()
        {
            return status + "\t" + statusMessage;
        }
    }


    public class ReportInfo
    {
        public string eid { get; set; }
        public List<Text_chan> text1 { get; set; }
        public List<Text_chan> text2 { get; set; }
        public List<Text_chan> text3 { get; set; }


        public override string ToString()
        {
            return text1[0].text + "\t" + text1[0].color + "\t" + text2[0].text + "\t" + text2[0].color + "\t" +
                   text3[0].text + "\t" + text3[0].color;
        }
    }

    public class Text_chan
    {
        public string text { get; set; }
        public string color { get; set; }
        public bool enabled { get; internal set; }
        public Font font { get; internal set; }
        public int fontSize { get; internal set; }
        public TextAnchor alignment { get; internal set; }
        public int preferredWidth { get; internal set; }
    }
    }
}