namespace SFramework.UI
{
    using Sirenix.OdinInspector;
    using TMPro;

    public class ReportDataItem : SerializedMonoBehaviour
    {
        public TextMeshProUGUI mName;
        public TextMeshProUGUI mScore;
        public TextMeshProUGUI mTime;
        public TextMeshProUGUI mDetailed;
        
        public ReportDataItem Assignment(string name, string score, string time, string detailed)
        {
            this.mName.text = name;
            this.mScore.text = score;
            this.mTime.text = time;
            this.mDetailed.text = detailed;
            return this;
        }
        
        
    }
}