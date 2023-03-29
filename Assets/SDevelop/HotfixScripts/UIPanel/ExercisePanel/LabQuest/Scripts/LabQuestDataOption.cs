

namespace SFramework.UI
{
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine.UI;
    public class LabQuestDataOption : SerializedMonoBehaviour
    {
        public Toggle toggle;
        public TMP_Text optionText;
        [ReadOnly]
        public ChoiceIndexStr choiceIndexStr;

        public virtual Toggle InitOption(Choice choice)
        {
            return null;
        }
    }

}
