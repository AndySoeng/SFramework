using Cysharp.Threading.Tasks;

namespace SFramework.UI
{
    using SFramework;
    using Michsky.UI.ModernUIPack;
    using UnityEngine;
    using UnityEngine.UI;

    public class NotificationsPanelScreenParam : UIOpenScreenParameterBase
    {
    }

    public class NotificationsPanelScreen : UIScreenBase
    {
        NotificationsPanelCtrl mCtrl;
        NotificationsPanelScreenParam mParam;

        protected override async UniTask OnLoadSuccess()
        {
            await base.OnLoadSuccess();
            mCtrl = mCtrlBase as NotificationsPanelCtrl;
            mParam = mOpenParam as NotificationsPanelScreenParam;
        }

        public static async UniTask<NotificationsPanelScreen> ShowNotifications(string title, string description, NPAppearMode mode = NPAppearMode.Sliding,
            NPLocation location = NPLocation.TR)
        {
            UIScreenBase sb = await SUIManager.Ins.OpenUI<NotificationsPanelScreen>();

            NotificationsPanelScreen notificationsPanelScreen = sb as NotificationsPanelScreen;
            NotificationManager notificationManager = notificationsPanelScreen.mCtrl.notificationManagerDic[mode + "Notification" + location];
            notificationManager.title = title;
            notificationManager.description = description;
            notificationManager.UpdateUI();
            notificationManager.OpenNotification();
            return notificationsPanelScreen;
        }

        private bool FloatNoticeIsShow = false;

        public static async UniTask<NotificationsPanelScreen> ShowFloatNotice(string content, Transform target, bool needFloow = false)
        {
            UIScreenBase sb = await SUIManager.Ins.OpenUI<NotificationsPanelScreen>();
            NotificationsPanelScreen notificationsPanelScreen = sb as NotificationsPanelScreen;
            if (notificationsPanelScreen.FloatNoticeIsShow) CloseFloatNotice(); //如果提示没关掉，就自己关掉

            notificationsPanelScreen.mCtrl.txt_FloatContent.text = content;


            Vector2 viewport = Camera.main.WorldToViewportPoint(target.position);
            Vector2 referenceResolution = notificationsPanelScreen.mCtrl.GetComponent<CanvasScaler>().referenceResolution;
            Vector2 uiPos = new Vector2(viewport.x * referenceResolution.x, viewport.y * referenceResolution.y);

            notificationsPanelScreen.mCtrl.go_FloatNotice.GetComponent<RectTransform>().anchoredPosition = uiPos;
            notificationsPanelScreen.mCtrl.go_FloatNotice.SetActive(true);
            if (needFloow)
            {
                notificationsPanelScreen.mCtrl.floatNoticeCoroutine.Add(
                    notificationsPanelScreen.mCtrl.StartCoroutine(notificationsPanelScreen.mCtrl.FloatNoticeFllowTarget(target)));
            }

            notificationsPanelScreen.FloatNoticeIsShow = true;
            return notificationsPanelScreen;
        }

        public static void CloseFloatNotice()
        {
            NotificationsPanelScreen notificationsPanelScreen = SUIManager.Ins.GetUI<NotificationsPanelScreen>();
            notificationsPanelScreen.mCtrl.go_FloatNotice.SetActive(false);
            notificationsPanelScreen.mCtrl.StopFloatCoroutine();
            notificationsPanelScreen.FloatNoticeIsShow = false;
        }


        private bool HoverNoticeIsShow = false;

        public static async UniTask<NotificationsPanelScreen > ShowHoverNotice(string content, bool needFloowMouse = true)
        {
            UIScreenBase sb= await SUIManager.Ins.OpenUI<NotificationsPanelScreen>();
            NotificationsPanelScreen notificationsPanelScreen = sb as NotificationsPanelScreen;
            if (notificationsPanelScreen.HoverNoticeIsShow) CloseHoverNotice(); //如果提示没关掉，就自己关掉

            notificationsPanelScreen.mCtrl.txt_HoverContent.text = content;

            Vector2 viewport = Camera.main.ScreenToViewportPoint(SUIManager.MouseScreenPosition);
            Vector2 referenceResolution = notificationsPanelScreen.mCtrl.GetComponent<CanvasScaler>().referenceResolution;
            Vector2 uiPos = new Vector2(viewport.x * referenceResolution.x, viewport.y * referenceResolution.y);

            notificationsPanelScreen.mCtrl.go_HoverNotice.GetComponent<RectTransform>().anchoredPosition = uiPos;
            notificationsPanelScreen.mCtrl.go_HoverNotice.SetActive(true);
            if (needFloowMouse)
            {
                notificationsPanelScreen.mCtrl.hoverNoticeCoroutine.Add(notificationsPanelScreen.mCtrl.StartCoroutine(notificationsPanelScreen.mCtrl.HoverNoticeFllowMouse()));
            }

            notificationsPanelScreen.HoverNoticeIsShow = true;
            return notificationsPanelScreen;
        }

        public static void CloseHoverNotice()
        {
            NotificationsPanelScreen notificationsPanelScreen = SUIManager.Ins.GetUI<NotificationsPanelScreen>();
            notificationsPanelScreen.mCtrl.go_HoverNotice.SetActive(false);
            notificationsPanelScreen.mCtrl.StopHoverCoroutine();
            notificationsPanelScreen.HoverNoticeIsShow = false;
        }


        private bool FingerPointingIsShow = false;

        public static async UniTask<NotificationsPanelScreen > ShowFingerPointing(Transform target, bool needFloow = false)
        {
            UIScreenBase sb= await SUIManager.Ins.OpenUI<NotificationsPanelScreen>();
            NotificationsPanelScreen notificationsPanelScreen = sb as NotificationsPanelScreen;
            if (notificationsPanelScreen.FingerPointingIsShow) CloseFingerPointing(); //如果提示没关掉，就自己关掉

            Vector2 viewport = Camera.main.WorldToViewportPoint(target.position);
            Vector2 referenceResolution = notificationsPanelScreen.mCtrl.GetComponent<CanvasScaler>().referenceResolution;
            Vector2 uiPos = new Vector2(viewport.x * referenceResolution.x, viewport.y * referenceResolution.y);

            notificationsPanelScreen.mCtrl.go_FingerPointing.GetComponent<RectTransform>().anchoredPosition = uiPos;
            notificationsPanelScreen.mCtrl.go_FingerPointing.SetActive(true);
            if (needFloow)
            {
                notificationsPanelScreen.mCtrl.fingerPointingCoroutine.Add(
                    notificationsPanelScreen.mCtrl.StartCoroutine(notificationsPanelScreen.mCtrl.FingerPointingFllowTarget(target)));
            }

            notificationsPanelScreen.FingerPointingIsShow = true;
            return notificationsPanelScreen;
        }

        public static void CloseFingerPointing()
        {
            NotificationsPanelScreen notificationsPanelScreen = SUIManager.Ins.GetUI<NotificationsPanelScreen>();
            notificationsPanelScreen.mCtrl.go_FingerPointing.SetActive(false);
            notificationsPanelScreen.mCtrl.StopFingerPointingCoroutine();
            notificationsPanelScreen.FingerPointingIsShow = false;
        }
    }
}