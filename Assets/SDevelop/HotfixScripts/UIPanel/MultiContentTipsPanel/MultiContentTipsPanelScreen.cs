using System;
using Cysharp.Threading.Tasks;
using Ex;
using TMPro;

namespace SFramework.UI
{
    using System.Collections.Generic;
    using SFramework;
    using Michsky.UI.ModernUIPack;
    using PathologicalGames;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine.Video;

    public class MultiContentTipsPanelScreenParam : UIOpenScreenParameterBase
    {
        public MultiContentTipsType tipType;

        //图片提示参数（大小1482*937）
        public Sprite spriteContent;
        public string spriteContentTitle;
        public string spriteContentDesc;

        //文字提示参数
        public string txtContent;

        //视频提示参数
        public string txtVideoName;

        //长图提示参数（宽1482）
        public Sprite spriteListViewContent;

        //分页窗口
        public List<PageContent> pageContents;
        public string pageContentsCloseBtnName;

        //全屏图片提示
        public Sprite fullScreenSpriteContent;
        public string str_FullScreenSpriteContentCloseButtonName;
        public string str_OpenUrl;

        //StringTableList
        public string stringTableListTitleName;
        public List<List<string>> stringTableListContents;
        public string stringTableListCloseBtnName;




        public Action closeCallBack;
    }

    public enum MultiContentTipsType
    {
        SpriteContent, //  大小1482*937
        TxtContent,
        VideoContent,
        SpriteListView, //宽1482
        PageWindow,
        FullScreenSpriteContent,
        StringTableList,
    }


    public class MultiContentTipsPanelScreen : UIScreenBase
    {
        MultiContentTipsPanelCtrl mCtrl;
        public  MultiContentTipsPanelScreenParam mParam;

        protected override async UniTask OnLoadSuccess()
        {
            await base.OnLoadSuccess();
            mCtrl = mCtrlBase as MultiContentTipsPanelCtrl;
            mParam = mOpenParam as MultiContentTipsPanelScreenParam;

            GameObject showObj = null;
            if (mParam.tipType == MultiContentTipsType.SpriteContent)
            {
                showObj = mCtrl.go_SpriteContent;
                mCtrl.ig_SpriteContent.sprite = mParam.spriteContent;
                if (string.IsNullOrEmpty(mParam.spriteContentTitle))
                {
                    mCtrl.txt_SpriteContentTitle.gameObject.SetActive(true);
                    mCtrl.txt_SpriteContentTitle.text = mParam.spriteContentTitle;
                }
                else
                    mCtrl.txt_SpriteContentTitle.gameObject.SetActive(false);

                if (string.IsNullOrEmpty(mParam.spriteContentDesc))
                {
                    mCtrl.txt_SpriteContentDesc.gameObject.SetActive(true);
                    mCtrl.txt_SpriteContentDesc.text = mParam.spriteContentDesc;
                }
                else
                    mCtrl.txt_SpriteContentDesc.gameObject.SetActive(false);
            }
            else if (mParam.tipType == MultiContentTipsType.TxtContent)
            {
                showObj = mCtrl.go_TxtContent;
                mCtrl.txt_txtContent.text = mParam.txtContent;
            }
            else if (mParam.tipType == MultiContentTipsType.VideoContent)
            {
                showObj = mCtrl.go_VedioConent;
                mCtrl.vp_VideoPlayer.source = VideoSource.Url;
                mCtrl.vp_VideoPlayer.url = Application.streamingAssetsPath + "/Video/" + mParam.txtVideoName + ".mp4";
                mCtrl.vp_VideoPlayer.Play();
                mCtrl.txt_VedioTitle.text = mParam.txtVideoName;
                mCtrl.vp_VideoPlayer.loopPointReached += mCtrl.CloseVideoContent;
            }
            else if (mParam.tipType == MultiContentTipsType.SpriteListView)
            {
                showObj = mCtrl.go_SpirteListView;
                mCtrl.ig_SpirteListViewContent.sprite = mParam.spriteListViewContent;
            }
            else if (mParam.tipType == MultiContentTipsType.PageWindow)
            {
                showObj = mCtrl.go_PageWindow;
                InitPageWindow(mParam.pageContents);
            }
            else if (mParam.tipType == MultiContentTipsType.FullScreenSpriteContent)
            {
                showObj = mCtrl.go_FullScreenSpriteContent;
                mCtrl.ig_FullScreenSpriteContent.sprite = mParam.fullScreenSpriteContent;
                if (!string.IsNullOrEmpty(mParam.str_FullScreenSpriteContentCloseButtonName))
                {
                    mCtrl.btn_CloseFullScreenSpriteContent.buttonText = mParam.str_FullScreenSpriteContentCloseButtonName;
                    mCtrl.btn_CloseFullScreenSpriteContent.UpdateUI();
                }
                mCtrl.btn_CloseFullScreenSpriteContent.buttonEvent.RemoveAllListeners();
                mCtrl.btn_CloseFullScreenSpriteContent.buttonEvent.AddListener(() =>
                {
                    mCtrl.StartCoroutine(mCtrl.Close(mParam.tipType,mParam.closeCallBack));
                });

                mCtrl.go_OpenUrl.SetActive(false);
                ExEventTrigger.RemoveAll(mCtrl.go_OpenUrl);
                if (!string.IsNullOrEmpty(mParam.str_OpenUrl))
                {
                    mCtrl.go_OpenUrl.SetActive(true);
                    ExEventTrigger.Add(mCtrl.go_OpenUrl,EventTriggerType.PointerClick, (arg0) =>
                    {
                        Debug.Log(mParam.str_OpenUrl);
                        ExBrowser.OpenURL(mParam.str_OpenUrl);
                    });
                }
            }
            else if (mParam.tipType==MultiContentTipsType.StringTableList)
            {
                showObj = mCtrl.go_StringTableList;
                InitStringTableList();
            }

            showObj.SetActive(true);
            showObj.GetComponent<Animator>().Play("ShowContent");

            //某些界面不需要点击背景关闭
            if (mParam.tipType != MultiContentTipsType.FullScreenSpriteContent ||
                (mParam.tipType ==MultiContentTipsType.PageWindow && string.IsNullOrEmpty(mParam.pageContentsCloseBtnName)) ||
                (mParam.tipType ==MultiContentTipsType.StringTableList && string.IsNullOrEmpty(mParam.stringTableListCloseBtnName))  )
            {
                ExEventTrigger.Add(mCtrl.go_Background, EventTriggerType.PointerClick, (arg) => { mCtrl.StartCoroutine(mCtrl.Close(mParam.tipType, mParam.closeCallBack)); });
            }
        }

        public static async UniTask<MultiContentTipsPanelScreen> ShowSpriteContent(Sprite content,Action closeCallBack=null)
        {
            UIScreenBase sb= await  SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.SpriteContent,
                spriteContent = content,
                closeCallBack = closeCallBack,
            });
            return sb as MultiContentTipsPanelScreen ;
        }

        public static  async UniTask<MultiContentTipsPanelScreen> ShowTxtContent(string content,Action closeCallBack=null)
        {
            UIScreenBase sb= await SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.TxtContent,
                txtContent = content,
                closeCallBack = closeCallBack,
            });
             return sb as MultiContentTipsPanelScreen ;
        }

        public static  async UniTask<MultiContentTipsPanelScreen> ShowVideoContent(string videoName,Action closeCallBack=null)
        {
            UIScreenBase sb= await   SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.VideoContent,
                txtVideoName = videoName,
                closeCallBack = closeCallBack,
            });
             return sb as MultiContentTipsPanelScreen ;
        }

        public static  async UniTask<MultiContentTipsPanelScreen> ShowSpriteListViewContent(Sprite content,Action closeCallBack=null)
        {
            UIScreenBase sb= await SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.SpriteListView,
                spriteListViewContent = content,
                closeCallBack = closeCallBack,
            });
             return sb as MultiContentTipsPanelScreen ;
        }

        public static  async UniTask<MultiContentTipsPanelScreen> ShowPageWindowContent(List<PageContent> content,string closeBtnName="",Action closeCallBack=null)
        {
            UIScreenBase sb= await SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.PageWindow,
                pageContents = content,
                pageContentsCloseBtnName = closeBtnName,
                closeCallBack = closeCallBack,
            });
             return sb as MultiContentTipsPanelScreen ;
        }
        
        public void InitPageWindow(List<PageContent> content)
        {
            Transform detailTxtContent = PoolManager.Pools["DetailPanelPool"].prefabs["DetailTxtContent"];
            Transform detailSpriteContent = PoolManager.Pools["DetailPanelPool"].prefabs["DetailSpriteContent"];
            Transform detailWindowButton = PoolManager.Pools["DetailPanelPool"].prefabs["DetailWindowButton"];

            List<int> buttonsList = new List<int>();
            if (!string.IsNullOrEmpty(mParam.pageContentsCloseBtnName))
            {
                if (mParam.pageContents.Count != 0)
                    buttonsList.Add(0);
                if (mParam.pageContents.Count == 1)
                    mCtrl.btn_PageWindowClose.gameObject.SetActive(true);
            }

            for (int i = 0; i < mParam.pageContents.Count; i++)
            {
                int index = i;
                WindowManager.WindowItem item = new WindowManager.WindowItem();
                item.windowName = mParam.pageContents[index].title;
                item.buttonObject = PoolManager.Pools["DetailPanelPool"].Spawn(detailWindowButton).gameObject;
                item.buttonObject.transform.SetParent(mCtrl.trans_PageWindowButtons);
                item.buttonObject.transform.ExUIResetXYZ(Vector3.zero);
                item.buttonObject.GetComponent<DetailWindowButton>().SetName(mParam.pageContents[index].title);
                item.buttonObject.GetComponent<Button>().onClick.RemoveAllListeners();
                item.buttonObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    mCtrl.windowManager_PageWindow.OpenPanel(mParam.pageContents[index].title);

                    if (!string.IsNullOrEmpty(mParam.pageContentsCloseBtnName))
                    {
                        if (!buttonsList.Contains(index))
                            buttonsList.Add(index);
                        if (buttonsList.Count == mParam.pageContents.Count)
                        {
                            mCtrl.btn_PageWindowClose.gameObject.SetActive(true);
                        }
                    }
                });
                if (mParam.pageContents[i].isSprite)
                {
                    item.windowObject = PoolManager.Pools["DetailPanelPool"].Spawn(detailSpriteContent).gameObject;
                    item.windowObject.GetComponent<DetailSpriteContent>().SetContent(mParam.pageContents[index].spriteContent);
                    item.windowObject.transform.SetParent(mCtrl.trans_PageWindowWindows);
                    item.windowObject.transform.ExUIResetXYZ(Vector3.zero).ExUIResetSizeDelta(Vector2.zero);
                }
                else
                {
                    item.windowObject = PoolManager.Pools["DetailPanelPool"].Spawn(detailTxtContent).ExUIResetZ().gameObject;
                    item.windowObject.GetComponent<DetailTxtContent>().SetContent(mParam.pageContents[index].txtContent);
                    item.windowObject.transform.SetParent(mCtrl.trans_PageWindowWindows);
                    item.windowObject.transform.ExUIResetXYZ(Vector3.zero).ExUIResetSizeDelta(Vector2.zero);
                }

                mCtrl.windowManager_PageWindow.windows.Add(item);
            }

            if (!string.IsNullOrEmpty(mParam.pageContentsCloseBtnName))
            {
                //mCtrl.btn_PageWindowClose.gameObject.SetActive(true);
                mCtrl.btn_PageWindowClose.buttonText = mParam.pageContentsCloseBtnName;
                mCtrl.btn_PageWindowClose.UpdateUI();
                mCtrl.btn_PageWindowClose.buttonEvent.RemoveAllListeners();
                mCtrl.btn_PageWindowClose.buttonEvent.AddListener(()=>{mCtrl.StartCoroutine(mCtrl.Close(mParam.tipType, mParam.closeCallBack));});
            }
        }
        
        public static  async UniTask<MultiContentTipsPanelScreen>  ShowFullScreenSpriteContent(Sprite content,string closeBtnName,Action closeCallBack=null)
        {
            UIScreenBase sb= await SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.FullScreenSpriteContent,
                fullScreenSpriteContent = content,
                str_FullScreenSpriteContentCloseButtonName = closeBtnName,
                closeCallBack = closeCallBack,
            });
            return sb as MultiContentTipsPanelScreen ;
        }
        
        public static  async UniTask<MultiContentTipsPanelScreen>  ShowFullScreenSpriteContent(Sprite content,string openurl,string closeBtnName,Action closeCallBack=null)
        {
            UIScreenBase sb= await SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.FullScreenSpriteContent,
                fullScreenSpriteContent = content,
                str_FullScreenSpriteContentCloseButtonName = closeBtnName,
                str_OpenUrl = openurl,
                closeCallBack = closeCallBack,
            });
            return sb as MultiContentTipsPanelScreen ;
        }

        
        public static  async UniTask<MultiContentTipsPanelScreen>  ShowStringTableList(string titleName,List<List<string >> datas,string closeBtnName=null,Action closeCallBack=null)
        {
            UIScreenBase sb= await  SUIManager.Ins.OpenUI<MultiContentTipsPanelScreen>(new MultiContentTipsPanelScreenParam()
            {
                tipType = MultiContentTipsType.StringTableList,
                stringTableListTitleName = titleName,
                stringTableListContents = datas,
                stringTableListCloseBtnName = closeBtnName,
                closeCallBack = closeCallBack,
            });
            return sb as MultiContentTipsPanelScreen ;
        }

        private void InitStringTableList()
        {
            SpawnPool pool= PoolManager.Pools["DetailPanelPool"];
            Transform stringTableListItem = pool.prefabs["StringTableListItem"];
            Transform stringTableListItemTxt = pool.prefabs["StringTableListItemTxt"];
            
            mCtrl.txt_StringTableListTitle.text = mParam.stringTableListTitleName;
            mCtrl.go_StringTableConfirmBtnGroup.SetActive(!string.IsNullOrEmpty(mParam.stringTableListCloseBtnName));
            mCtrl.btn_StringTableConfirmBtn.buttonEvent.RemoveAllListeners();
            if (!string.IsNullOrEmpty(mParam.stringTableListCloseBtnName))
            {
                mCtrl.btn_StringTableConfirmBtn.buttonText = mParam.stringTableListCloseBtnName;
                mCtrl.btn_StringTableConfirmBtn.UpdateUI();
                mCtrl.btn_StringTableConfirmBtn.buttonEvent.AddListener(() =>
                {
                    mCtrl.StartCoroutine(mCtrl.Close(mParam.tipType, mParam.closeCallBack));
                });
            }
            
            for (int i = 0; i < mParam.stringTableListContents.Count; i++)
            {
                Transform dataParent= pool.Spawn(stringTableListItem, mCtrl.trans_StringTableItems).ExUIResetZ()
                    .ExSetAsLastSibling();
                for (int j = 0; j < mParam.stringTableListContents[i].Count; j++)
                {
                    pool.Spawn(stringTableListItemTxt, dataParent).ExUIResetZ()
                        .ExSetAsLastSibling().GetComponent<TMP_Text>().text = mParam.stringTableListContents[i][j];
                }
            }
        }


        
    }
}
