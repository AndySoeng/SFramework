namespace SFramework.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using SFramework;
    using Michsky.UI.ModernUIPack;
    using TMPro;
    using UnityEngine;

    public class ModalWindowPanelCtrl : UICtrlBase
    {
        public Dictionary<string, ModalWindowManager> ModalWindowManagerDic = new Dictionary<string, ModalWindowManager>();


        public TMP_Text txt_Countdown;


        /// <summary>
        /// 延时显示
        /// </summary>
        /// <param name="activeGameObject"></param>
        /// <param name="countdownLength"></param>
        /// <returns></returns>
        public IEnumerator CountdownActive(GameObject activeGameObject, int countdownLength = 3)
        {
            if (countdownLength > 0)
            {
                txt_Countdown.gameObject.SetActive(true);
                int time = countdownLength;
                for (int i = 0; i < countdownLength; i++)
                {
                    txt_Countdown.text = time.ToString();
                    yield return new WaitForSeconds(1);
                    time -= 1;
                }

                txt_Countdown.gameObject.SetActive(false);
            }

            activeGameObject.SetActive(true);
        }
    }
}