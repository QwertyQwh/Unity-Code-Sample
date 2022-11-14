using MyProtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Table.Structure;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using ZhFramework.Engine;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.Resource;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{


    public class UnitAchievementCollectionInfo : UIWidget
    {
        #region Feilds
        [Hotfix] private XButton m_BtnYes;
        [Hotfix] private XButton m_BtnNo;
        [Hotfix] private RectTransform m_RootMask;
        [Hotfix] private RectTransform m_RootItemInfo;
        [Hotfix] private XText m_TxtTitle;
        [Hotfix] private XText m_TxtProgress;
        [Hotfix] private XText m_TxtBtnYes;
        [Hotfix] private XText m_TxtBtnNo;
        private UnitItemInfo m_ItemInfo;
        public  AchievementCollectionInfo Info;
        private bool m_isCommonReward;
        #endregion Feilds



        #region Methods

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_ItemInfo = CreateWidget<UnitItemInfo>(m_RootItemInfo, true);
        }


        public async Task<bool> SetCollectionData(AchievementCollectionInfo info)
        {
            Info = info;
            m_RootMask.SetActiveEx(info.IsComplete);
            m_TxtTitle.text = $"{TableAchievementText.Find(info.data.NameKey)} { ModuleManager.Instance.Get<AchievementModule>().GetAchievementTertiaryTabProgressText(info.TabId)}";
            m_TxtProgress.text = TableAchievementText.Find(info.data.Describe);
            if (info.data.SpecialReward == 0)
            {
                var rewards = await UIHelper.GetSingleDropRewards(info.data.ShowReward[0]);
                m_ItemInfo.SetItemInfo(rewards[0].ItemId, rewards[0].Count);
                m_TxtBtnYes.text = TableSysPropertyText.Common_Btn_RedeemReward;
                m_TxtBtnNo.text = TableSysPropertyText.Common_Btn_Go;
            }
            else
            {
                var rewardId = info.data.ShowReward[0];
                m_ItemInfo.SetItemInfo(rewardId, 1);
                m_TxtBtnYes.text = TableSysPropertyText.Common_Btn_Activate;
                m_TxtBtnNo.text = TableSysPropertyText.Common_Btn_Activate;
            }
            SetStatus(info);
            return true;
        }
        private void SetStatus(AchievementCollectionInfo info)
        {
            m_isCommonReward = info.data.SpecialReward == 0;
            if (info.IsComplete)
            {
                m_RootMask.SetActiveEx(true);
                m_BtnYes.SetActiveEx(false);
                m_BtnNo.SetActiveEx(false);
                return;
            }
            var module = ModuleManager.Instance.Get<AchievementModule>();
            if (module.IsTertiaryTabCompleted(info.TabId))
            {
                m_RootMask.SetActiveEx(false);
                m_BtnYes.SetActiveEx(true);
                m_BtnNo.SetActiveEx(false);
            }
            else
            {
                m_RootMask.SetActiveEx(false);
                m_BtnYes.SetActiveEx(false);
                m_BtnNo.SetActiveEx(true);

            }
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_BtnNo.onClick.AddListener(OnButtonNoClicked);
            m_BtnYes.onClick.AddListener(OnButtonYesClicked);
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_BtnNo.onClick.RemoveAllListeners();
            m_BtnYes.onClick.RemoveAllListeners();
        }
        private void OnButtonNoClicked()
        {
            if (m_isCommonReward)
                PageFloatingTips.ShowFloatingTip(TableSysPropertyText.Achievement_Collection_Tips_Common);
            else
                PageFloatingTips.ShowFloatingTip(TableSysPropertyText.Achievement_Collection_Tips_Special);
        }
        private void OnButtonYesClicked()
        {
            if (m_isCommonReward)
            {
                var module = ModuleManager.Instance.Get<AchievementModule>();
                module.SubmitSubAchievement(Info);
                m_RootMask.SetActiveEx(true);
                m_BtnYes.SetActiveEx(false);
                m_BtnNo.SetActiveEx(false);
            }
            else
            {
                if (null == Info || null == Info.data) { return; }
                GameJump.OpenJumpRoute(Info.data.GameJump);
            }

        }

        public void SetCollectedIfAble()
        {
            if (m_isCommonReward)
            {
                m_RootMask.SetActiveEx(true);
                m_BtnYes.SetActiveEx(false);
                m_BtnNo.SetActiveEx(false);
            }
        }


        #endregion Methods
    }
}
