using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace SFramework.UI
{
    using SFramework;
    using UnityEngine;

    public class SchedulePanelScreenParam : UIOpenScreenParameterBase
    {
        public List<string> strs_ScheduleNotice;
        public float druation = 3f;
        public Action callBack;
    }

    public class SchedulePanelScreen : UIScreenBase
    {
        SchedulePanelCtrl mCtrl;
        SchedulePanelScreenParam mParam;

        protected override void OnLoadSuccess()
        {
            mCtrl = mCtrlBase as SchedulePanelCtrl;
            mParam = mOpenParam as SchedulePanelScreenParam;

            InitComponent();
            mCtrl.StartCoroutine(Wait());
        }

        private void InitComponent()
        {
            mCtrl.img_Schedule.fillAmount = 0;
            mCtrl.txt_Schedule.text = "0%";
            mCtrl.txt_ScheduleNotice.text = "";
        }


        public IEnumerator Wait()
        {
            float pieceTime = mParam.druation / mParam.strs_ScheduleNotice.Count;
            for (int i = 0; i < mParam.strs_ScheduleNotice.Count; i++)
            {
                mCtrl.txt_ScheduleNotice.text = mParam.strs_ScheduleNotice[i];
                float percentage = (float)(i + 1) / mParam.strs_ScheduleNotice.Count;
                mCtrl.img_Schedule.DOFillAmount(percentage, pieceTime).SetEase(Ease.Linear).OnUpdate(() =>
                {
                    mCtrl.txt_Schedule.text = (int) (mCtrl.img_Schedule.fillAmount * 100) + "%";
                });
                yield return new WaitForSeconds(pieceTime);
            }

            mParam.callBack?.Invoke();
            SUIManager.Ins.CloseUI<SchedulePanelScreen>();
        }
    }
}