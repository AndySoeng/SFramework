using UnityEngine;

namespace SFramework.UI
{
    using UnityEngine.UI;

    public class LabQuestDataImageOption : LabQuestDataOption
    {
        public Image optionImage;

        public override Toggle InitOption(Choice choice)
        {
            base.InitOption(choice);
            SpriteChoice spriteChoice = choice as SpriteChoice;
            optionText.text = spriteChoice.option.ToString();
            optionImage.sprite = spriteChoice.choiceSprite;
            choiceIndexStr = spriteChoice.option;
            return toggle;
        }
    }
}