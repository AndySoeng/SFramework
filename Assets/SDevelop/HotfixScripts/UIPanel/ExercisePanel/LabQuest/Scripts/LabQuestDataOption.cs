

using Michsky.UI.ModernUIPack;
using UnityEngine;

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
            toggle.group = null;
            toggle.isOn = false;
            toggle.GetComponent<Animator>().Play("Toggle Off");
            return toggle;
        }
    }

}
