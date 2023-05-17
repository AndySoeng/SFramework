namespace SFramework.UI
{
    using UnityEngine.UI;

    public class LabQuestDataTextOption : LabQuestDataOption
    {
        public override Toggle InitOption(Choice choice)
        {
            base.InitOption(choice);
            TxtChoice txtChoice = choice as TxtChoice;
            optionText.text = txtChoice.option + "." + txtChoice.choiceContent;
            choiceIndexStr = txtChoice.option;
            return toggle;
        }
    }
}