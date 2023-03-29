using System;

namespace SFramework.UI
{
    using System.Collections;
    using SFramework;
    using Michsky.UI.ModernUIPack;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;

    public class MultiContentTipsPanelCtrl : UICtrlBase
    {
        public GameObject go_Background;
        public GameObject go_SpriteContent;
        public TMP_Text txt_SpriteContentTitle;
        public TMP_Text txt_SpriteContentDesc;
        public Image ig_SpriteContent;
        public GameObject go_TxtContent;
        public TMP_Text txt_txtContent;

        public GameObject go_VedioConent;

        //public DisplayUGUI V_VedioContent;
        public TMP_Text txt_VedioTitle;
        public VideoPlayer vp_VideoPlayer;

        public GameObject go_SpirteListView;
        public ScrollRect sr_SpirteListView;
        public Image ig_SpirteListViewContent;

        public GameObject go_PageWindow;
        public Transform trans_PageWindowButtons;
        public Transform trans_PageWindowWindows;
        public WindowManager windowManager_PageWindow;
        public ButtonManagerBasicWithIcon btn_PageWindowClose;
        
        
        public GameObject go_FullScreenSpriteContent;
        public Image ig_FullScreenSpriteContent;
        public ButtonManagerBasic btn_CloseFullScreenSpriteContent;
        public GameObject go_OpenUrl;


        public GameObject go_StringTableList;
        public TMP_Text txt_StringTableListTitle;
        public Transform trans_StringTableItems;
        public GameObject go_StringTableConfirmBtnGroup;
        public ButtonManager btn_StringTableConfirmBtn;

        public IEnumerator Close(MultiContentTipsType isSpriteContent,Action closeCallBack=null)
        {
            GameObject currentClose = null;
            if (isSpriteContent == MultiContentTipsType.SpriteContent)
            {
                currentClose = go_SpriteContent;
            }
            else if (isSpriteContent == MultiContentTipsType.TxtContent)
            {
                currentClose = go_TxtContent;
            }
            else if (isSpriteContent == MultiContentTipsType.VideoContent)
            {
                currentClose = go_VedioConent;
                vp_VideoPlayer.loopPointReached -= CloseVideoContent;
            }
            else if (isSpriteContent == MultiContentTipsType.SpriteListView)
            {
                currentClose = go_SpirteListView;
            }
            else if (isSpriteContent == MultiContentTipsType.PageWindow)
            {
                currentClose = go_PageWindow;
            }
            else if (isSpriteContent == MultiContentTipsType.FullScreenSpriteContent)
            {
                currentClose = go_FullScreenSpriteContent;
            }else if (isSpriteContent==MultiContentTipsType.StringTableList)
            {
                currentClose = go_StringTableList;
            }

            currentClose.GetComponent<Animator>().Play("HideContent");
            float closeAnimationTime = currentClose.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(closeAnimationTime);
            closeCallBack?.Invoke();
            SUIManager.Ins.CloseUI<MultiContentTipsPanelScreen>();
        }

        public void CloseVideoContent(VideoPlayer source)
        {
            StartCoroutine(Close(MultiContentTipsType.VideoContent,SUIManager.Ins.GetUI<MultiContentTipsPanelScreen>().mParam.closeCallBack));
        }
    }
}