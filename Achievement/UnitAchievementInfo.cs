using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Table.Structure;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.Resource;
using ZhFramework.Unity.UI;
using MyProtos;

namespace Game.Runtime
{


    public class UnitAchievementInfo : UIWidget
    {
        #region Feilds
        [Hotfix] private XButton m_BtnYes;
        [Hotfix] private XButton m_BtnNo;
        [Hotfix] private XImage m_ImgSpecialRewardIcon;
        [Hotfix] private RectTransform m_RootMask;
        [Hotfix] private RectTransform m_RootItemInfo;
        [Hotfix] private XText m_TxtTitle;
        [Hotfix] private XText m_TxtProgress;
        [Hotfix] private XText m_TxtBtnYes;
        [Hotfix] private XText m_TxtBtnNo;
        [Hotfix] private XText m_TxtDate;
        [Hotfix] private GameObject m_RootDate;
        public AchievementInfo Info;
        private List<UnitItemInfo> m_items = new List<UnitItemInfo>();


        #endregion Feilds



        #region Methods

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            for (int i = 0; i < 3; i++)
            {
                m_items.Add( CreateWidget<UnitItemInfo>(m_RootItemInfo, false));
            }
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_BtnYes.onClick.AddListener(OnBtnYesClicked);
            m_BtnNo.onClick.AddListener(OnBtnNoClicked);
        }


        //可前往
        private void OnBtnNoClicked()
        {
            if (null == Info|| null == Info.Data) { return; }
            GameJump.OpenJumpRoute(Info.Data.GameJump);
        }

        private void OnBtnYesClicked()
        {

            var module = ModuleManager.Instance.Get<AchievementModule>();
            module.SubmitAchievement(Info.Id);
            SetCompleteMask();

        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_BtnYes.onClick.RemoveAllListeners();
            m_BtnNo.onClick.RemoveAllListeners();
        }
        public async Task<bool> SetAchievementInfo(AchievementInfo info)
        {
            Info = info;
            switch (info.Status)
            {
                case EAchievementStatus.Redeemed:
                    SetCompleteMask();
                    break;
                case EAchievementStatus.Actionable:
                    SetActionable();
                    break;
                case EAchievementStatus.Redeemable:
                    SetRedeemable();
                    break;
            }
            var title = "";// $"{TableAchievementText.Find(m_Info.Data.NameKey)}({m_Info.Curprogress}/{m_Info.TargetProgress})";
            if(info.Type == EAchievementType.Recommended)
            {
                title = $"{TableSysPropertyText.Achievement_Recommend}{title}";
            }
            if (info.Data.HasSpecialReward > 0)
                m_ImgSpecialRewardIcon.SetActiveEx(true);
            else
                m_ImgSpecialRewardIcon.SetActiveEx(false);
            m_TxtTitle.text = title;
            var rewards = await UIHelper.GetSingleDropRewards(Info.Data.ShowReward);
            for(int i = 0; i<m_items.Count; i++)
            {
                if(i< rewards.Count)
                {
                    m_items[i].SetItemInfo(rewards[i].ItemId, rewards[i].Count);
                    m_items[i].SetActive(true);
                }
                else
                {
                    m_items[i].SetActive(false);
                }
            }
            m_TxtProgress.text = string.Format(TableAchievementText.Find(info.Data.Describe), Info.TargetProgress);
            m_TxtProgress.text += "   ";
            m_TxtProgress.text += string.Format(TableStyleText.Achievement_Complete, info.Curprogress, info.TargetProgress);
            var createTimeStamp = info.CompletedDate;
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var createTime = startTime.AddMilliseconds(createTimeStamp);
            m_TxtDate.text = createTime.ToShortDateString();
            return true;
        }

        private void SetCompleteMask()
        {
            m_BtnYes.SetActiveEx(false);
            m_BtnNo.SetActiveEx(false);
            m_RootMask.SetActiveEx(true);
            m_RootDate.SetActiveEx(true);
        }


        public void SetCollectedIfAble()
        {
            if (m_BtnYes.IsActive())
            {
                SetCompleteMask();
            }
        }
        private void SetRedeemable()
        {
            m_BtnYes.SetActiveEx(true);
            m_BtnNo.SetActiveEx(false);
            m_RootMask.SetActiveEx(false);
            m_RootDate.SetActiveEx(true);

        }

        private void SetActionable()
        {
            m_BtnNo.SetActiveEx(true);
            m_BtnYes.SetActiveEx(false);
            m_RootMask.SetActiveEx(false);
            m_RootDate.SetActiveEx(false);

        }

        #endregion Methods
    }
}