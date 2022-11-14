using System.Collections.Generic;
using Table.Structure;
using UnityEngine;
using UnityEngine.UI;
using ZhFramework.Unity.UI;
using Myproto;
using MyProtos;
using System;
using ZhFramework.Unity.Misc;
using ZhFramework.Engine.Msic;

namespace Game.Runtime
{
    [UISettings(UILayer.Default, UIContext.Blur | UIContext.Popup | UIContext.Tips)]
    public class DialogAdventureReputation : CommonDialog_List
    {

        [Hotfix]
        private Transform m_TransPool;

        [Hotfix]
        private RectTransform m_TransToggle;

        [Hotfix] private Slider m_StrongholdSlider;
        [Hotfix] private XText m_StrongholdTxtName;
        [Hotfix] private XImage m_StrongholdImgIcon;
        [Hotfix] private XText m_StrongholdTxtProgress;
        [Hotfix] private XText m_StrongholdTxtLevel;

        private UIPoolList<UnitToggleV0, VoTabData> m_TabList;
        [Hotfix] private RectTransform m_Item1Root;
        [Hotfix] private RectTransform m_Item2Root;
        [Hotfix] private RectTransform m_Item3Root;
        [Hotfix] private RectTransform m_Item4Root;
        [Hotfix] private RectTransform m_Item5Root;
        [Hotfix] private RectTransform m_Item6Root;
        [Hotfix] private XText m_TxtReputationLevel0;
        [Hotfix] private XText m_TxtReputationLevel1;
        [Hotfix] private XText m_TxtReputationLevel2;
        [Hotfix] private XText m_TxtReputationLevel3;
        [Hotfix] private XText m_TxtReputationLevel4;
        [Hotfix] private XText m_TxtReputationLevel5;
        [Hotfix] private Slider m_ReputationSlider;
        [Hotfix] private XImage m_ImgBanner;

        private UnitItemInfo[] m_ItemData;
        private XText[] m_ReputationLevelTexts;
        private Dictionary<int, List<UnitAdventureReputationTab>> m_LevelupDict = new Dictionary<int, List<UnitAdventureReputationTab>>();
        private Dictionary<UnitItemInfo,int> m_ItemInfoDict = new Dictionary<UnitItemInfo, int>();
        private RectTransform[] m_ItemRoots;

        private int m_lastTabId;
        private int m_CurTabId;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_ReputationLevelTexts = new XText[] { m_TxtReputationLevel0, m_TxtReputationLevel1, m_TxtReputationLevel2, m_TxtReputationLevel3, m_TxtReputationLevel4, m_TxtReputationLevel5 };
            m_ItemRoots = new RectTransform[] { m_Item1Root, m_Item2Root, m_Item3Root, m_Item4Root, m_Item5Root,m_Item6Root };
            m_ItemData = new UnitItemInfo[m_ItemRoots.Length];
            InitTab();
            SetReputationIcon();
            InitReputationItems();
            SetupLevelupData();
            SwitchReputationTabs();

        }


        private void InitTab()
        {
            m_TabList = new UIPoolList<UnitToggleV0, VoTabData>(m_TransPool, m_TransToggle, false);
            AdventureManager.Instance.SetTabData();
            m_TabList.SetData(AdventureManager.Instance.TabsData);
            for(int i = 0; i<m_TabList.Count; i++)
            {
                ((UnitToggleV0)m_TabList[i]).SetRedDot();
            }
            m_TabList.selectedIndex = 0;
            m_lastTabId = GetCurrentTabId();
            m_CurTabId = m_lastTabId;
        }
        public async void GetStrongholdReward(int strongholdId, int level)
        {
            //注意：目前显示的奖励是配置表里的，领取的奖励是服务器的，两者可能不同
            PageWaiting.StartWaiting();
            var req = new GetStrongholdLevelRewardReq() {
                StrongholdId = strongholdId,
                Level = level
            };
            var ack = await req.Invoke<GetStrongholdLevelRewardAck>();
            m_ItemData[ack.msg.Item.StrongholdLv - 1].ShowMark();
            m_ItemData[ack.msg.Item.StrongholdLv - 1].SetClickEvent(null);
            m_ItemData[ack.msg.Item.StrongholdLv - 1].ShowTipsType = ItemShowTipsType.Auto;
            AdventureManager.Instance.GetStrongholdData(GetCurrentTabId()).strongholdRewardCollected.Add(level);
            RedDotManager.Instance.SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][ack.msg.Item.StrongholdLv - 1], 0);
            PageWaiting.StopWaiting();
        }


        private int GetCurrentTabId()
        {
            return ((UnitToggleV0)m_TabList.selectedItem).GetTabData().ID;
        }
        private int GetCurrentTabRedDotId()
        {
            return ((UnitToggleV0)m_TabList.selectedItem).GetTabData().RedDotId;
        }


        private void InitReputationItems()
        {
            var stronghold = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId());
            var collected = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId()).strongholdRewardCollected;
            for (int i = 0; i < m_ItemRoots.Length; i++)
            {
                var data = GetLevelUpDataFromTabId(i+1);
                m_ItemData[i] = CreateWidget<UnitItemInfo>(m_ItemRoots[i], true, data.Reward[0][0], data.Reward[0][1]);
                m_ItemData[i].SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][i]);
                m_ItemInfoDict.Add(m_ItemData[i], i + 1);
                if(i < stronghold.strongholdLv)
                {
                    if (collected.Contains(i))
                    {
                        m_ItemData[i].ShowMark();
                    }
                    else
                    {
                        RedDotManager.Instance.SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][i],1);
                    }
                }
                else
                {
                    RedDotManager.Instance.SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][i], 0);
                }

            }



        }
        private void SetReputationItems()
        {
            var stronghold = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId());
            var collected = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId()).strongholdRewardCollected;
            for (int i = 0; i < m_ItemRoots.Length; i++)
            {
                var data = GetLevelUpDataFromTabId(i+1);
                m_ItemData[i].SetData(data.Reward[0][0], data.Reward[0][1]);
                m_ItemData[i].SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][i]);
                if (i < stronghold.strongholdLv)
                {
                    if (collected.Contains(i+1))
                    {
                        m_ItemData[i].ShowMark();
                        m_ItemData[i].SetClickEvent(null);
                        m_ItemData[i].ShowTipsType = ItemShowTipsType.Auto;
                        RedDotManager.Instance.SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][i], 0);
                    }
                    else
                    {
                        m_ItemData[i].ShowMark(false);
                        m_ItemData[i].SetClickEvent(OnItemClicked);
                        m_ItemData[i].ShowTipsType = ItemShowTipsType.DontShow;
                        RedDotManager.Instance.SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][i], 1);
                    }
                    m_ItemData[i].ShowTipsType = ItemShowTipsType.DontShow;
                }
                else
                {
                    m_ItemData[i].ShowMark(false);
                    m_ItemData[i].ShowTipsType = ItemShowTipsType.Auto;
                    m_ItemData[i].SetClickEvent(null);
                    RedDotManager.Instance.SetRedDot(AdventureManager.Instance.RewardDict[GetCurrentTabId()][i], 0);
                }
            }
        }

        private void OnItemClicked(UnitItemInfo info)
        {
            if(!m_ItemInfoDict.TryGetValue(info,out var level))
            {
                CLogger.Error("奖励不存在，无法领取");
                return;
            }
            GetStrongholdReward(GetCurrentTabId(), level);
        }

        private void SetReputationIcon()
        {
            var stronghold = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId());
            var data = TableExtensions.GetStrongholdLevelUpData(stronghold.strongholdId, stronghold.strongholdLv);
            var strongholddata = TableExtensions.GetTableStrongholdDescData(stronghold.strongholdId);
            if (TableExtensions.GetStrongholdLevelMax() > stronghold.strongholdLv)
            {
                m_StrongholdSlider.maxValue = data.TownExp;
                m_StrongholdSlider.value = stronghold.strongholdExp;
                m_StrongholdTxtProgress.text = $"{stronghold.strongholdExp}/{data.TownExp}";
            }
            else
            {
                m_StrongholdSlider.maxValue = 1;
                m_StrongholdSlider.value = 1;
                m_StrongholdTxtProgress.text = TableSysPropertyText.StrongholdLevelMax;
            }


            m_StrongholdImgIcon.SetSpriteSafe(strongholddata.Icon);
            m_StrongholdTxtName.text = strongholddata.GetStrongholdName();
            m_StrongholdTxtLevel.text = TableExtensions.GetStrongholdLevelStyleName(data.Level);

            m_ImgBanner.SetSpriteSafe(strongholddata.Banner);

            m_ReputationSlider.maxValue = 6;
            m_ReputationSlider.value = stronghold.strongholdLv;
            for(int i = 0; i< TableExtensions.GetStrongholdLevelMax(); i++)
            {
                m_ReputationLevelTexts[i].text = TableExtensions.GetStrongholdLevelName(i+1);
            }
            
        }

        private TableStrongholdLevelUp.Data GetcurrentLevelUpData()
        {
            var stronghold = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId());
            return TableExtensions.GetStrongholdLevelUpData(stronghold.strongholdId, stronghold.strongholdLv);
        }

        private TableStrongholdLevelUp.Data GetLevelUpDataFromTabId(int Level)
        {
            return TableExtensions.GetStrongholdLevelUpData(GetCurrentTabId(), Level);
        }

        private void SetupLevelupData()
        {
           foreach(var stronghold in AdventureManager.Instance.LastUnlockedStrongholdIds)
            {
                m_LevelupDict.Add(stronghold, new List<UnitAdventureReputationTab>());
                for (int i = 0; i < m_ItemRoots.Length; i++){
                    var data = TableExtensions.GetStrongholdLevelUpData(stronghold, i+1);
                    m_LevelupDict[stronghold].Add(CreateWidget<UnitAdventureReputationTab>(m_ContentTransform, false, data));
                }
            }
           
        }




        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_TabList.OnSelected.AddListener(OnTabListClick);
        }


        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_TabList.OnSelected.RemoveAllListeners();
        }

        private void OnTabListClick(int index, VoTabData data)
        {
            SwitchReputationTabs();
        }

        private void SetReputationTabs(int Id, bool val)
        {
            for(int i = 0; i < 6; i++)
            {
                m_LevelupDict[Id][i].SetActive(val);
            }
        }
        private void SwitchReputationTabs()
        {
            m_lastTabId = m_CurTabId;
            m_CurTabId = GetCurrentTabId();
            SetReputationIcon();
            SetReputationItems();
            SetReputationTabs(m_lastTabId, false);
            SetReputationTabs(m_CurTabId, true);
        }


    }
}