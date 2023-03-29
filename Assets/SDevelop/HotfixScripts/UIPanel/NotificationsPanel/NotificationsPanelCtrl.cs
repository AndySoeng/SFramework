namespace SFramework.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using SFramework;
    using Michsky.UI.ModernUIPack;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class NotificationsPanelCtrl : UICtrlBase
    {
        public Dictionary<string, NotificationManager> notificationManagerDic =
            new Dictionary<string, NotificationManager>();

        public GameObject go_FloatNotice;
        public TMP_Text txt_FloatContent;
        [HideInInspector] public List<Coroutine> floatNoticeCoroutine = new List<Coroutine>();

        public GameObject go_HoverNotice;
        public TMP_Text txt_HoverContent;
        [HideInInspector] public List<Coroutine> hoverNoticeCoroutine = new List<Coroutine>();


        public GameObject go_FingerPointing;
        [HideInInspector] public List<Coroutine> fingerPointingCoroutine = new List<Coroutine>();


        public IEnumerator FloatNoticeFllowTarget(Transform target)
        {
            while (true)
            {
                if (target == null || Camera.main == null)
                {
                    for (int i = 0; i < floatNoticeCoroutine.Count; i++)
                    {
                        StopCoroutine(floatNoticeCoroutine[i]);
                    }

                    floatNoticeCoroutine.Clear();
                }

                Vector2 viewport = Camera.main.WorldToViewportPoint(target.position);
                Vector2 referenceResolution = GetComponent<CanvasScaler>().referenceResolution;
                Vector2 uiPos = new Vector2(viewport.x * referenceResolution.x, viewport.y * referenceResolution.y);

                go_FloatNotice.GetComponent<RectTransform>().anchoredPosition = uiPos;
                yield return new WaitForEndOfFrame();
            }
        }

        public void StopFloatCoroutine()
        {
            for (int i = 0; i < floatNoticeCoroutine.Count; i++)
            {
                StopCoroutine(floatNoticeCoroutine[i]);
            }

            floatNoticeCoroutine.Clear();
        }

        public IEnumerator HoverNoticeFllowMouse()
        {
            while (true)
            {
                if (Camera.main == null)
                {
                    StopHoverCoroutine();
                }

                Vector2 viewport = Camera.main.ScreenToViewportPoint(SUIManager.MouseScreenPosition);
                Vector2 referenceResolution = GetComponent<CanvasScaler>().referenceResolution;
                Vector2 uiPos = new Vector2(viewport.x * referenceResolution.x, viewport.y * referenceResolution.y);

                go_HoverNotice.GetComponent<RectTransform>().anchoredPosition = uiPos;
                yield return new WaitForEndOfFrame();
            }
        }

        public void StopHoverCoroutine()
        {
            for (int i = 0; i < hoverNoticeCoroutine.Count; i++)
            {
                StopCoroutine(hoverNoticeCoroutine[i]);
            }

            hoverNoticeCoroutine.Clear();
        }


        public IEnumerator FingerPointingFllowTarget(Transform target)
        {
            while (true)
            {
                if (target == null || Camera.main == null)
                {
                    for (int i = 0; i < fingerPointingCoroutine.Count; i++)
                    {
                        StopCoroutine(fingerPointingCoroutine[i]);
                    }

                    fingerPointingCoroutine.Clear();
                }

                Vector2 viewport = Camera.main.WorldToViewportPoint(target.position);
                Vector2 referenceResolution = GetComponent<CanvasScaler>().referenceResolution;
                Vector2 uiPos = new Vector2(viewport.x * referenceResolution.x, viewport.y * referenceResolution.y);

                go_FingerPointing.GetComponent<RectTransform>().anchoredPosition = uiPos;
                yield return new WaitForEndOfFrame();
            }
        }


        public void StopFingerPointingCoroutine()
        {
            for (int i = 0; i < fingerPointingCoroutine.Count; i++)
            {
                StopCoroutine(fingerPointingCoroutine[i]);
            }

            fingerPointingCoroutine.Clear();
        }
    }


    public enum NPAppearMode
    {
        Fading,
        Popup,
        Sliding,
    }

    public enum NPLocation
    {
        TL,
        TR,
        BL,
        BR,
    }
}