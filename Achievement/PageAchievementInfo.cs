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

namespace Game.Runtime
{

    [UISettings(UILayer.Default, mode: UIMutexMode.HideOther, addBackStack: true)]
    public class PageAchievementInfo : UIPanel
    {
        #region Feilds

        [Hotfix] private RectTransform m_RootIconBig;
        [Hotfix] private RectTransform m_RootBtnCollectAllMask;
        [Hotfix] private XImage m_ImgBg;
        [Hotfix] private XScrollRect m_ScrollAchievements;
        [Hotfix] private XButton m_BtnCollectAll;
        [Hotfix] private XText m_TxtCollectAll;

        private UITabList m_TabList;
        private List<VoTabData> m_VoData;
        private int m_ParentId;
        private int m_GrandParentId;
        private List<AchievementInfo> m_AchievementInfos;
        private UnitAchievementCollectionInfo m_CollectionWidget;
        private List<UnitAchievementInfo> m_AchievementInfoWidgets = new List<UnitAchievementInfo>();
        private UnitAchievementIconLarge m_Icon;

        #endregion Feilds



        #region Methods

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_ParentId = (int)args[0];
            m_GrandParentId = (int)args[1];

            InitTabs();
            m_ImgBg.SetSpriteSafe(TableGameAsset.Achievement_InfoBg);
            UIHelper.AttachUnitPageBack(this, UIHelper.GetAchievementSubTabName(m_ParentId), "", OnCloseClicked).SetHelp(TableSysTipText.Tips_Achievement);
            m_TabList.selectedIndex = ModuleManager.Instance.Get<AchievementModule>().GetPageAchievementInfoDefaultIndex(m_GrandParentId,m_ParentId);

            m_Icon = CreateWidget<UnitAchievementIconLarge>(m_RootIconBig, true);
            m_Icon.SetData(m_GrandParentId, m_ParentId, m_TabList.selectedIndex + 1);
                m_CollectionWidget = CreateWidget<UnitAchievementCollectionInfo>(m_ScrollAchievements.content, true);
            m_TxtCollectAll.text = TableSysPropertyText.CB_Btn_OnekeyReceive;
        }

        private async void SetListInfo()
        {
            var module = ModuleManager.Instance.Get<AchievementModule>();
            m_AchievementInfos = module.GetAchievementInfosFromTabGroup(GetTabGroupId());
            m_AchievementInfos.Sort(AchievementModule.AchievementInfoComparer);
            if (module.CheckAchievementCollection(GetTabGroupId(), out var data))
            {
                await m_CollectionWidget.SetCollectionData(data);
                m_CollectionWidget.SetActive(true);
            }
            else
            {
                m_CollectionWidget.SetActive(false);
            }
            int dif = m_AchievementInfos.Count - m_AchievementInfoWidgets.Count;
            for(int i = 0; i< dif; i++)
            {
                m_AchievementInfoWidgets.Add(CreateWidget<UnitAchievementInfo>(m_ScrollAchievements.content, true));
            }
            for(int i = 0; i< m_AchievementInfoWidgets.Count; i++)
            {
                if (i < m_AchievementInfos.Count)
                {
                    await m_AchievementInfoWidgets[i].SetAchievementInfo(m_AchievementInfos[i]);
                    m_AchievementInfoWidgets[i].SetActive(true);
                }
                else
                {
                    m_AchievementInfoWidgets[i].SetActive(false);
                }
            }
            if (CheckIfAnyTabCollectable())
            {
                m_RootBtnCollectAllMask.SetActive(false);
            }else
                m_RootBtnCollectAllMask.SetActive(true);
        }
        private void OnCloseClicked()
        {
            GameURL.GlobalBack(this);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            m_TabList.selectedIndex = ModuleManager.Instance.Get<AchievementModule>().GetPageAchievementInfoDefaultIndex(m_GrandParentId, m_ParentId);
            SetListInfo();
        }
        private AchievementTabGroup GetTabGroupId()
        {
           return new AchievementTabGroup() { First = m_GrandParentId, Secondary = m_ParentId, Tertiary = m_TabList.selectedIndex + 1 };
        }

        private void InitTabs()
        {
            var tertiaryTabs = TableExtensions.GetTableAchievementTertiaryTabs(m_ParentId);
            m_VoData = new List<VoTabData>();
            for (int i = 0; i <tertiaryTabs.Length; i++)
            {
                int id = i + 1;
                m_VoData.Add(new VoTabData()
                {
                    Name = UIHelper.GetAchievementTertiaryTabTitle(id),
                    ID = id,
                    RedDotId = ModuleManager.Instance.Get<AchievementModule>().TertiaryRedDotIds[new AchievementTabGroup() { First = m_GrandParentId, Secondary = m_ParentId, Tertiary = id}]
                });
            }
            m_TabList = CreateWidget<UITabList>(Transform, true, m_VoData);
            if(m_VoData.Count <= 1)
                m_TabList.SetActive(false);
            m_TabList.SetIconStyle(TableGameAsset.UnitTabAchievement,false);
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_TabList.OnSelected += (OnTabListClicked);
            m_BtnCollectAll.onClick.AddListener(OnCollectAllClicked);
        }

        private void OnCollectAllClicked()
        {
            var module = ModuleManager.Instance.Get<AchievementModule>();

            if (module.CheckAchievementCollection(GetTabGroupId(), out var data))
            {
                if (CheckIfAnyTabCollectable())
                {
                    module.SubmitSecondaryAllAchievements(data);
                    foreach (var widget in m_AchievementInfoWidgets)
                    {
                        if (widget.IsActive)
                        {
                            widget.SetCollectedIfAble();
                        }
                    }
                    m_CollectionWidget.SetCollectedIfAble();
                    m_RootBtnCollectAllMask.SetActive(true);
                }
                else
                {
                    PageFloatingTips.ShowFloatingTip(TableSysPropertyText.ReceiveALL);
                }
            }


        }

        private bool CheckIfAnyTabCollectable()
        {
            foreach (var widget in m_AchievementInfoWidgets)
            {
                if (widget.IsActive)
                {
                    if (widget.Info.Status == EAchievementStatus.Redeemable)
                        return true;
                }
            }
            if (m_CollectionWidget.Info.data.SpecialReward == 0 && ModuleManager.Instance.Get<AchievementModule>().IsTertiaryTabCompleted(m_CollectionWidget.Info.TabId) && !m_CollectionWidget.Info.IsComplete)
                return true;
            return false;
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_TabList.OnSelected -= (OnTabListClicked);
            m_BtnCollectAll.onClick.RemoveAllListeners();

        }
        private void OnTabListClicked(int index, VoTabData data)
        {
            SetListInfo();
            m_Icon.SetData(m_GrandParentId, m_ParentId, index + 1);


        }
        



        #endregion Methods
    }
}