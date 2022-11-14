using System.Collections.Generic;
using Table.Structure;
using UnityEngine;
using UnityEngine.UI;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{
    [UISettings(UILayer.Default, UIContext.Default | UIContext.DisableBlank, mode: UIMutexMode.HideOther, addBackStack: true)]
    public class PageAdventure : UIPanel
    {
        [Hotfix] private Transform m_TabPoolRoot;
        [Hotfix] private RectTransform m_NewMissionRoot;
        [Hotfix] private XScrollList m_ScrollList;
        [Hotfix] private XImage m_ImgAdventureLevelText;
        [Hotfix] private XImage m_ImgAdventureLevel;
        [Hotfix] private XImage m_ImgAdventureCoinIcon;
        [Hotfix] private Slider m_AdventurerLevelslider;
        [Hotfix] private XText m_AdventurerLevelsliderProgress;
        [Hotfix] private XText m_TxtCoin;
        [Hotfix] private XButton m_BtnShop;
        [Hotfix] private XButton m_BtnReward;
        [Hotfix] private XButton m_BtnFilter;

        [Hotfix] private XText m_TxtCommission;
        [Hotfix] private XText m_TxtFilter;
        [Hotfix] private RectTransform m_RegionTabsRoot;
        [Hotfix] private RectTransform m_SpecialMissionRoot;
        [Hotfix] private RectTransform m_SpecialMissionRootCopy;
        [Hotfix] private Transform m_reputation2Root;
        [Hotfix] private Transform m_reputation1Root;

        [Hotfix] private XText m_CommisionDescription;
        
        private UIPoolList<UnitToggleH3, UnitToggleH3_Left, UnitToggleH3_Right, VoTabData> m_TabList; 
        private VoTabData[] m_TabsData;
        private DialogTaskFilter m_DialogTaskFilter = null;
        private List<UnitAdventureReputation> m_reputationIcons =new List<UnitAdventureReputation>();
        private Dictionary<int, List<MissionInfo>[]> m_DictEntrustMissions = new Dictionary<int, List<MissionInfo>[]>();

        private int previousDifficulty = 0;//上一个选择的任务难度
        private int currentDifficulty = 0;//当前选择的任务难度

        private List<MissionInfo> m_CurMissions = new List<MissionInfo>();     //当前显示任务组
        private List<AdventureStronghold> m_StrongholdTabs; //据点页签
        private int m_selectedTabId = 1; //当前选择的页签
        private List<MissionInfo> m_MissionPromoted = new List<MissionInfo>(); // 晋级任务
        private List<MissionInfo> m_MissionReward = new List<MissionInfo>(); // 指名任务
        private List<UnitAdventureSpecialTask> m_specialMissions = new List<UnitAdventureSpecialTask>(); // 左下角的委托任务面板
        private Dictionary<int, UnitAdventureSpecialTask> m_DictTweenerObjects = new Dictionary<int, UnitAdventureSpecialTask>(); //动画里在飞的假面板
        private bool m_IsPlayingAnimation = false; //是不是在播放动画
        private Dictionary<int, int> m_DictStrongholdIdToTabId = new Dictionary<int, int>(); //
        private int m_DefaultTabIndex = 1; //默认选中的页签
        private MissionInfo m_specialMissionWaiting = null; //当前因为其他面板在显示而不能跳弹窗的委托任务面板的任务信息
        private AdventureStronghold m_strongholdWaiting = null; //当前因为奖励面板在显示而不能显示据点升级页面的据点信息

        private UnitRedDot rewardDot;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            InitMiscellany();
            m_ScrollList.Init(CreateWidget, ShowWidget, ClickWidget);
            m_StrongholdTabs = AdventureManager.Instance.GetStrongholdTabs();
            if (m_StrongholdTabs.Count == 0)
            {
                CLogger.Error("所有地区未解锁");
                return;
            }
            m_DefaultTabIndex = ModuleManager.Instance.Get<MissionModule>().GetCurMissionStrongHoldID();
            if (m_DefaultTabIndex <= 0)
            {
                m_DefaultTabIndex = 1;
            }
            InitTabs();
            InitTasktabList();
            AdventureManager.Instance.SetTabData();
            previousDifficulty = AdventureManager.Instance.AdventurerData.adventuererLevelData.DifficultyLevel - 1;
            currentDifficulty = previousDifficulty;
            m_TxtFilter.text = MissionExtensions.GetDifficultText(currentDifficulty + 1);
            rewardDot = CreateWidget<UnitRedDot>(m_BtnReward.transform, true, (int)RedDotType.Adventure_Reputation);
            UpdateFilter(GetCurrentTabId(), GetCurrentTabId(), currentDifficulty);
            SetCurrentTabEntrustMissions();
            SetReputationIcons();
            InitSpecialMissions();
            UpdateSpecialMissionsFromServer();
            AdventureManager.Instance.onStrongholdLevelUpNew += OnStrongholdLevelUp;
            AdventureManager.Instance.onStrongholdExpUpNew += OnStrongholdExpUp;
            AdventureManager.Instance.onAdventuerExpUpNew += OnAdventurerExpUp;
            AdventureManager.Instance.onAdventuerLevelUpNew += OnAdventurerLevelUp;
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();

            if (m_StrongholdTabs.Count == 0)
            {
                return;
            }

            GameTransfer.OnTransferChanged += OnTransferChanged;
            ModuleManager.Instance.Get<MissionModule>().OnMissionChanged += OnMissionChanged;
            ModuleManager.Instance.Get<BackpackModule>().OnCurrencyChanged += OnCurrencyChanged;
            ModuleManager.Instance.Get<BackpackModule>().RewardAdventureCoinsThisWeek.Regist(OnCoinMaxChanged);

            m_TabList.OnSelected.AddListener(OnTabListClick);
            m_BtnShop.onClick.AddListener(OnShopClicked);
            m_BtnReward.onClick.AddListener(OnRewardClicked);
            m_BtnFilter.onClick.AddListener(OnFilterClicked);
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            if (m_StrongholdTabs.Count == 0)
            {
                return;
            }
            GameTransfer.OnTransferChanged -= OnTransferChanged;
            ModuleManager.Instance.Get<MissionModule>().OnMissionChanged -= OnMissionChanged;
            ModuleManager.Instance.Get<BackpackModule>().OnCurrencyChanged -= OnCurrencyChanged;
            ModuleManager.Instance.Get<BackpackModule>().RewardAdventureCoinsThisWeek.UnRegist(OnCoinMaxChanged);

            m_TabList.OnSelected.RemoveAllListeners();
            m_BtnShop.onClick.RemoveAllListeners();
            m_BtnReward.onClick.RemoveAllListeners();
            m_BtnFilter.onClick.RemoveAllListeners();
        }

        #region 服务器事件处理
        private void OnStrongholdLevelUp(AdventureStronghold stronghold)
        {
            m_strongholdWaiting = stronghold;
            if (UIHelper.CheckInCommonReward(out var panel))
            {
                panel.OnClosed += OnShowDialogAdventureReputationLevelup;
            }
            else
            {
                OnShowDialogAdventureReputationLevelup();
            }
        }

        private void OnShowDialogAdventureReputationLevelup()
        {
            var panel = UIManager.Instance.CreatePanel<DialogAdventureReputationLevelUp>(m_strongholdWaiting);
            panel.OnClosed += OnDialogAdventureReputationLevelupClosed;
            if (m_specialMissionWaiting != null)
            {
                panel.OnClosed += OnShowSpecialMissionPanel;
            }
            SetReputationIcons();
        }

        private void OnDialogAdventureReputationLevelupClosed()
        {
            m_strongholdWaiting = null;
        }

        private void OnAdventurerExpUp(long val)
        {
            m_AdventurerLevelslider.value = val;
            var data = TableExtensions.GetTableAdventurerLevelData(AdventureManager.Instance.AdventurerData.adventurerLv);
            var expDisplay = val <= data.NeedExp ? val : data.NeedExp;
            m_AdventurerLevelsliderProgress.text = $"{val}/{data.NeedExp}";
        }

        private void OnAdventurerLevelUp(Adventurer adventurer, int oldLevel)
        {
            SetAdventurerInfo();
        }

        private void OnStrongholdExpUp(int id, long exp)
        {
            if (GetCurrentTabId() == id|| id == AdventureManager.Instance.GetMagicLandId())
            {
                SetReputationIcons();
                //m_reputation1Slider.value = exp;
                //m_reputation1TxtProgress.text = $"{exp}/{TableExtensions.GetStrongholdLevelUpData(id, AdventureManager.Instance.GetStrongholdData(id).strongholdLv).TownExp}";
            }
        }
        #endregion

        #region 初始化面板数据 
        private void InitSpecialMissions()
        {
            m_specialMissions = new List<UnitAdventureSpecialTask>();
            for (int i = 0; i < 5; i++)
            {
                m_specialMissions.Add(CreateWidget<UnitAdventureSpecialTask>(m_SpecialMissionRoot, false));
            }
        }


        private void InitTasktabList()
        {
            foreach (var stronghold in m_StrongholdTabs)
            {
                m_DictEntrustMissions.Add(stronghold.strongholdId, new List<MissionInfo>[TableGlobalNum.MaxAdventureQualityCount]);
                for (int i = 0; i < TableGlobalNum.MaxAdventureQualityCount; i++)
                {
                    m_DictEntrustMissions[stronghold.strongholdId][i] = new List<MissionInfo>();
                }
            }
        }


        private void InitMiscellany()
        {
            UIHelper.AttachUnitPageBack(this, TableSysPropertyText.AdventurerTitle, "", OnCloseClicked)
                .SetHelp(TableSysTipText.Tips_Adventurer);
            SetAdventurerInfo();
            SetAdventureCoinInfo();
            SetCommissionData();
            PlayAdventureAudio(AdventureManager.Instance.AdventurerData.adventuererLevelData.OpenUiMusic);
        }


        private void InitTabs()
        {
            m_StrongholdTabs = AdventureManager.Instance.GetStrongholdTabs();
            m_TabsData = new VoTabData[m_StrongholdTabs.Count];
            for (int i = 0; i < m_StrongholdTabs.Count; i++)
            {
                var id = m_StrongholdTabs[i].strongholdId;
                m_TabsData[i] = new VoTabData()
                {
                    Name = TableExtensions.GetStrongholdName(m_StrongholdTabs[i].strongholdId),
                    ID = id
                };
                m_DictStrongholdIdToTabId.Add(id, i);
            }
            m_TabList = new UIPoolList<UnitToggleH3, UnitToggleH3_Left, UnitToggleH3_Right, VoTabData>(m_TabPoolRoot, m_RegionTabsRoot, true);
            m_TabList.SetData(m_TabsData);
            if (m_DictStrongholdIdToTabId.TryGetValue(m_DefaultTabIndex, out var tabid))
                m_TabList.selectedIndex = tabid;
            else
                CLogger.Error("委托任务不在已解锁区域内");
            m_selectedTabId = GetCurrentTabId();
        }
        #endregion

        private void UpdateSpecialMissionsFromServer()
        {
            var missions = ModuleManager.Instance.Get<MissionModule>().GetAdventurerMissions();
            if (missions.Count > 5)
            {
                CLogger.Error("非委托任务数量超过五个！");
                return;
            }
            m_MissionPromoted.Clear();
            m_MissionReward.Clear();
            foreach (var mission in missions)
            {
                if (mission.CheckMissionType(EMissionType.Promoted))
                    m_MissionPromoted.Add(mission);

                if (mission.CheckMissionType(EMissionType.Reward))
                    m_MissionReward.Add(mission);
            }
            m_MissionReward.Sort(MissionModule.RewardMissionComparer);
            for (int i = 0; i < m_MissionPromoted.Count; i++)
            {
                m_specialMissions[i].SetData(m_MissionPromoted[i]);
                m_specialMissions[i].SetActive(true);
            }
            for (int i = m_MissionPromoted.Count; i < m_MissionReward.Count + m_MissionPromoted.Count; i++)
            {
                m_specialMissions[i].SetData(m_MissionReward[i - m_MissionPromoted.Count]);
                m_specialMissions[i].SetActive(true);
            }
            for (int i = missions.Count; i < 5; i++)
            {
                m_specialMissions[i].SetActive(false);
            }
        }

        private void UpdateSpecialMissions()
        {
            m_MissionReward.Sort(MissionModule.RewardMissionComparer);
            for (int i = 0; i < m_MissionPromoted.Count; i++)
            {
                m_specialMissions[i].SetData(m_MissionPromoted[i]);
                m_specialMissions[i].SetActive(true);
            }
            for (int i = m_MissionPromoted.Count; i < m_MissionReward.Count + m_MissionPromoted.Count; i++)
            {
                m_specialMissions[i].SetData(m_MissionReward[i - m_MissionPromoted.Count]);
                m_specialMissions[i].SetActive(true);
            }
            for (int i = m_MissionPromoted.Count + m_MissionReward.Count; i < 5; i++)
            {
                m_specialMissions[i].SetActive(false);
            }
        }



        protected override void OnDestroy()
        {
            PlayAdventureAudio(AdventureManager.Instance.AdventurerData.adventuererLevelData.CloseUiMusic);
            UIManager.Instance.GetPanel<DialogAdventureStoreInlet>()?.Close();
        }


        private void SetCurrentTabEntrustMissions()
        {
            for (int i = 0; i < TableGlobalNum.MaxAdventureQualityCount; i++)
            {
                m_DictEntrustMissions[GetCurrentTabId()][i].Clear();
            }
            var missionInfos = ModuleManager.Instance.Get<MissionModule>().GetAdventurerEntrustMissions(GetCurrentTabId(), AdventureManager.Instance.GetStrongholdData(GetCurrentTabId()).strongholdMissions.ToArray());
            foreach (var mission in missionInfos)
            {
                //如果在冒险者面板冒险者升级后需要读取高等级任务 这个时候服务器不会推送新任务 所以必须存储所有难度的任务
                //if (mission.Data.DifficultyLevel <= TableExtensions.GetTableAdventurerLevelData(AdventureManager.Instance.AdventurerData.adventurerLv).DifficultyLevel )
                //{
                //    m_DictEntrustMissions[mission.StrongHoldID][mission.Data.DifficultyLevel - 1].Add(mission);
                //}
                m_DictEntrustMissions[GetCurrentTabId()][mission.Data.DifficultyLevel - 1].Add(mission);
            }
            SetDisplayMissions(GetCurrentTabId(), currentDifficulty + 1);
        }

        private void SetDisplayMissions(int strongholdId, int difficulty)
        {
            m_CurMissions.Clear();
            m_CurMissions.AddRange(m_DictEntrustMissions[strongholdId][difficulty - 1]);
            m_CurMissions.Sort(MissionModule.MissionComparer);

            m_ScrollList.ShowList(GetWidgetCount());
        }

        private void SetReputationIcons()
        {
            if (m_reputationIcons.Count < 2)
            {
                var strongholddata = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId());
                m_reputationIcons.Add(CreateWidget<UnitAdventureReputation>(m_reputation1Root, true, strongholddata));
                if (AdventureManager.Instance.isMagicLandUnlocked() && GetCurrentTabId() == 1)
                {
                    var magicLandData = AdventureManager.Instance.GetStrongholdData(AdventureManager.Instance.GetMagicLandId());
                    m_reputationIcons.Add(CreateWidget<UnitAdventureReputation>(m_reputation2Root, true, magicLandData));
                }
                else
                {
                    m_reputationIcons.Add(CreateWidget<UnitAdventureReputation>(m_reputation2Root, false, strongholddata));
                }
                return;
            }
            else
            {
                var strongholddata = AdventureManager.Instance.GetStrongholdData(GetCurrentTabId());
                m_reputationIcons[0].SetData(strongholddata);
                if (AdventureManager.Instance.isMagicLandUnlocked() && GetCurrentTabId() == 1)
                {
                    var magicLandData = AdventureManager.Instance.GetStrongholdData(AdventureManager.Instance.GetMagicLandId());
                    m_reputationIcons[1].SetData(magicLandData);
                    m_reputationIcons[1].SetActive(true);
                }
                else
                    m_reputationIcons[1].SetActive(false); 
            }

        }


        private void SetAdventureCoinInfo()
        {
            m_ImgAdventureCoinIcon.SetSpriteSafe(TableItemData.Find((int)ECurrencyType.AdventureCoin).GetItemIcon());
            var adventureCoinCount = ModuleManager.Instance.Get<BackpackModule>().RewardAdventureCoinsThisWeek.Value;
            var adventureCoinMaxCount = TableExtensions.GetAdventurerCoinMax(AdventureManager.Instance.AdventurerData.adventurerLv);
            m_TxtCoin.text = $"{adventureCoinCount}/{adventureCoinMaxCount}";
        }

        private void SetAdventurerInfo()
        {
            var data = AdventureManager.Instance.AdventurerData.adventuererLevelData;

            m_ImgAdventureLevel.SetSpriteSafe(data.GradeIcon);
            m_ImgAdventureLevelText.SetSpriteSafe(data.GradeTextIcon);
            if (AdventureManager.Instance.AdventurerData.adventurerLv < TableExtensions.GetAdventurerLevelMax())
            {
                m_AdventurerLevelslider.maxValue = data.NeedExp;
                m_AdventurerLevelslider.value = AdventureManager.Instance.AdventurerData.adventurerExp;
                var expDisplay = AdventureManager.Instance.AdventurerData.adventurerExp <= data.NeedExp ? AdventureManager.Instance.AdventurerData.adventurerExp : data.NeedExp;
                m_AdventurerLevelsliderProgress.text = $"{expDisplay}/{data.NeedExp}";
            }
            else
            {
                m_AdventurerLevelslider.maxValue = 1;
                m_AdventurerLevelslider.value = 1;
                m_AdventurerLevelsliderProgress.text = TableSysPropertyText.StrongholdLevelMax;
            }
        }

        private int GetCurrentTabId()
        {
            return m_TabsData[m_TabList.selectedIndex].ID;
        }


        private void UpdateFilter(int oldtabid, int newtabid, int newDifficulty)
        {
            previousDifficulty = currentDifficulty;
            currentDifficulty = newDifficulty;
            //SetFilteredTabs(oldtabid, previousDifficulty, false);
            //SetFilteredTabs(newtabid, currentDifficulty, true);
            SetDisplayMissions(GetCurrentTabId(), currentDifficulty + 1);
        }

        private UnitAdventureTaskTab CreateWidget()
        {
            return CreateWidget<UnitAdventureTaskTab>(m_ScrollList.content);
        }

        private void ClickWidget(int index, UnitAdventureTaskTab obj)
        {
            var data = GetWidgetData(index);
            if (data == null) { return; }
            OnTaskTabClicked(data);
        }

        private void ShowWidget(int index, UnitAdventureTaskTab widget)
        {
            var data = GetWidgetData(index);
            if (data == null) { return; }
            widget.SetData(data);
        }

        private MissionInfo GetWidgetData(int index)
        {
            if (null == m_CurMissions) { return null; }
            if (index < 0 || index >= m_CurMissions.Count)
            {
                return null;
            }
            return m_CurMissions[index];
        }

        private int GetWidgetCount()
        {
            return (null == m_CurMissions ? 0 : m_CurMissions.Count);
        }

        private int GetWidgetIndex()
        {
            if (null == m_CurMissions) { return 0; }
            return 0;
        }

        private void OnFilterClicked()
        {
            m_DialogTaskFilter = UIManager.Instance.CreatePanel<DialogTaskFilter>(currentDifficulty + 1, AdventureManager.Instance.AdventurerData.adventuererLevelData.DifficultyLevel);
            m_DialogTaskFilter.OnOk += OnFilterConfirmed;
        }

        private void OnFilterConfirmed()
        {
            if (m_DialogTaskFilter.QualityFilter <= AdventureManager.Instance.AdventurerData.adventurerLv)
            {
                UpdateFilter(GetCurrentTabId(), GetCurrentTabId(), m_DialogTaskFilter.QualityFilter - 1);
                m_TxtFilter.text = MissionExtensions.GetDifficultText(m_DialogTaskFilter.QualityFilter);

                SetDisplayMissions(GetCurrentTabId(), currentDifficulty + 1);
                return;
            }

            PageFloatingTips.ShowFloatingTip(TableSysPropertyText.DifficultyConditionNotMet);
        }

        private void SetCommissionData()
        {
            var itemData = TableExtensions.GetTableItemData((int)ECurrencyType.Commission);
            var commissionCount = UIHelper.GetItemCount(ECurrencyType.Commission);
            var commissionMax = TableExtensions.GetCommissionMax();
            m_TxtCommission.text = $"<sprite name=\"{itemData.GetItemIcon()}\">{itemData.GetItemName()}({commissionCount}/{commissionMax})";
            m_CommisionDescription.text = TableSysPropertyText.CommisionDescription;
        }

        private void OnTabListClick(int index, VoTabData data)
        {
            if (m_selectedTabId == data.ID) { return; }

            UpdateFilter(m_selectedTabId, data.ID, currentDifficulty);
            m_selectedTabId = data.ID;
            SetReputationIcons();

            SetCurrentTabEntrustMissions();
        }

        private void OnTaskTabClicked(MissionInfo mission)
        {
            if (!mission.IsValid()) { return; }
            if (mission.CheckAcceptedEntrustMissionInOtherDifficulty())
            {
                PageFloatingTips.ShowFloatingTip(TableSysPropertyText.AdventureAcceptedEntrustMissionInOtherDifficulty);
                return;
            }

            if (mission.CheckMissionStatus(EMissionStatus.Completed))
            {
                PageFloatingTips.ShowFloatingTip(TableSysPropertyText.AdventurerTaskFinish);
            }
            else
            {
                ModuleManager.Instance.Get<MissionModule>().ExecuteAdventurerMission(mission);
            }
        }

        private void OnShopClicked()
        {

            UIManager.Instance.CreatePanel<DialogAdventureStoreInlet>();
        }

        private void OnRewardClicked()
        {
            UIManager.Instance.CreatePanel<DialogAdventureReputation>();
        }

        private void OnCloseClicked()
        {
            GameURL.GlobalBack();
        }

        private void OnCoinMaxChanged(long oldval, long newval)
        {
            SetAdventureCoinInfo();
        }

        private void OnCurrencyChanged(int itemID, long oldValue, long newValue)
        {
            if ((ECurrencyType)itemID == ECurrencyType.Commission)
                SetCommissionData();
            if ((ECurrencyType)itemID == ECurrencyType.AdventureCoin)
                SetAdventureCoinInfo();
        }

        private void OnTransferChanged(ETransferProcedure procedure, ETransferResult result)
        {
            if (procedure == ETransferProcedure.None) { return; }

            OnCloseClicked();
        }

        private void OnMissionChanged(object sender, MissionEventArgs arg)
        {
            if (arg.Status == EMissionEventStatus.OnSelected || arg.Status == EMissionEventStatus.OnUnlock)
            {
                return;
            }

            if (arg.Status == EMissionEventStatus.OnNav)
            {
                OnCloseClicked();
                return;
            }

            if (null == arg.Mission || !arg.Mission.IsValid()) { return; }

            CheckPromotedMission(arg.Mission);
            CheckRewardMission(arg.Mission);
            CheckEntrustMission(arg.Mission);
        }

        private void CheckPromotedMission(MissionInfo mission) //晋级
        {

            if (!mission.CheckMissionType(EMissionType.Promoted)) { return; }

            bool isAdded = true;
            int i = 0;
            for (i = 0; i < m_MissionReward.Count + m_MissionPromoted.Count; i++)
            {
                if (m_specialMissions[i].MissionInfo.MissionID == mission.MissionID)
                {
                    isAdded = false;
                    break;
                }
            }
            if (isAdded)
            {
                CreateNewSpecialMissionPanel(mission);
            }
            else if (mission.CheckMissionStatus(EMissionStatus.Completed))
            {
                CLogger.Log("删除任务");
                DeleteMission(EMissionType.Promoted, mission);
                UpdateSpecialMissions();
            }
            else
            {
                CLogger.Log("刷新任务");
                m_MissionPromoted[i] = mission;
                UpdateSpecialMissions();
            }
        }

        private void CreateNewSpecialMissionPanel(MissionInfo mission)
        {
            if (m_IsPlayingAnimation || m_specialMissionWaiting !=null)
                return;
            CLogger.Log("加入任务");
            m_specialMissionWaiting = mission;
            if (UIHelper.CheckInCommonReward(out var panel))
            {
                panel.OnClosed += OnShowSpecialMissionPanel;
            }
            else
            {
                OnShowSpecialMissionPanel();
            }
        }

        private void OnShowSpecialMissionPanel()
        {
            if (m_strongholdWaiting != null)
            {
                return;
            }
            UIManager.Instance.CreatePanel<DialogAdventureNewMission>(m_specialMissionWaiting).onMissionClosed += OnNewMissionClosed;
            m_IsPlayingAnimation = true;
            m_specialMissionWaiting = null;
        }

        private void DeleteMission(EMissionType missionType, MissionInfo missionToDelete)
        {
            int index = -1;
            switch (missionType)
            {
                case EMissionType.Promoted:
                    index = m_MissionPromoted.FindIndex(mission => mission.MissionID == missionToDelete.MissionID);
                    if (index == -1)
                    {
                        CLogger.Error("没找到晋级任务");
                        return;
                    }
                    m_MissionPromoted.RemoveAt(index);
                    break;
                case EMissionType.Reward:
                    index = m_MissionReward.FindIndex(mission => mission.MissionID == missionToDelete.MissionID);
                    if (index == -1)
                    {
                        CLogger.Error("没找到指名任务");
                        return;
                    }
                    m_MissionReward.RemoveAt(index);
                    m_MissionReward.Sort(MissionModule.RewardMissionComparer);
                    break;
            }
        }

        private void CheckRewardMission(MissionInfo mission)   //指名
        {
            if (!mission.CheckMissionType(EMissionType.Reward)) { return; }
            bool isAdded = true;
            int i = 0;
            for (i = 0; i < m_MissionReward.Count + m_MissionPromoted.Count; i++)
            {
                if (m_specialMissions[i].MissionInfo.MissionID == mission.MissionID)
                {
                    isAdded = false;
                    break;
                }
            }
            if (isAdded)
                CreateNewSpecialMissionPanel(mission);
            else if (mission.CheckMissionStatus(EMissionStatus.Completed))
            {
                CLogger.Log("删除任务");
                DeleteMission(EMissionType.Reward, mission);
                UpdateSpecialMissions();
            }
            else
            {
                CLogger.Log("刷新任务");
                m_MissionReward[i - m_MissionPromoted.Count] = mission;
                UpdateSpecialMissions();
            }
        }

        private void OnNewMissionClosed(MissionInfo mission)
        {
            if (m_MissionPromoted.Count + m_MissionReward.Count >= 5)
            {
                CLogger.Error("非委托任务超过五个");
                return;
            }
            int insertIndex = AddAndGetNewMissionPosition(mission);
            if (insertIndex == -1)
            {
                CLogger.Error("新任务不是非委托任务");
                return;
            }

            UpdateSpecialMissions();
            Rect rect = (m_specialMissions[insertIndex].Transform as RectTransform).rect;
            var rectsize = new Vector2(rect.width, rect.height/2.0f);

            var padding = m_SpecialMissionRoot.GetComponent<HorizontalLayoutGroup>().spacing ;

            //用上面两个数据计算传送位置！！！！
            int activeCount = insertIndex;
            for (int i = insertIndex + 1; i < 5; i++)
            {
                if (!m_specialMissions[i].IsActive)
                {
                    activeCount = i - 1;
                    break;
                }
            }

            for (int i = insertIndex + 1; i < activeCount+1; i++)
            {

                var tempPushedback = CreateWidget<UnitAdventureSpecialTask>(m_specialMissions[i].Transform, true);

                tempPushedback.SetData(m_specialMissions[i].MissionInfo);
                tempPushedback.Transform.SetParent(m_SpecialMissionRootCopy.transform, true);
                var pushedbackTween = tempPushedback.Transform.gameObject.GetComponent<TweenPosition>();
                pushedbackTween.from = new Vector2(rectsize.x * (i-1 + 0.5f) + (i-1) * padding, -rectsize.y); 
                pushedbackTween.to = new Vector2(rectsize.x * (i + 0.5f) + i * padding, -rectsize.y); 
                m_DictTweenerObjects.Add(tempPushedback.Transform.gameObject.GetInstanceID(), tempPushedback);
                pushedbackTween.enabled = true;
                pushedbackTween.onFinished.Add(OnTweenFinished);
                pushedbackTween.Play(true);
            }
            var tmp = CreateWidget<UnitAdventureSpecialTask>(m_NewMissionRoot, true);
            tmp.SetData(mission);
            tmp.Transform.SetParent(m_SpecialMissionRootCopy.transform, true);
            var tmptween = tmp.Transform.gameObject.GetComponent<TweenPosition>();
            tmptween.enabled = true;
            tmptween.from = (tmp.Transform as RectTransform).anchoredPosition;
            tmptween.to = new Vector2(rectsize.x*(insertIndex+0.5f)+insertIndex*padding, -rectsize.y);
            tmptween.onFinished.Add(OnTweenFinished);
            tmptween.Play(true);
            m_DictTweenerObjects.Add(tmp.Transform.gameObject.GetInstanceID(), tmp);
            for(int i = insertIndex; i < 5; i++)
            {
                m_specialMissions[i].SetActive(false);
            }

        }

        private void SetSpecialMissions(int index, bool active)
        {
            for (int i = 0; i <= index; i++)
            {
                m_specialMissions[i].SetActive(active);
            }
        }

        private void OnTweenFinished(GameObject go)
        {
            int id = go.GetInstanceID();
            if (m_DictTweenerObjects.TryGetValue(id, out var value))
            {

                for (int i = 0; i < m_MissionReward.Count + m_MissionPromoted.Count; i++)
                {
                    if (m_specialMissions[i].MissionInfo.MissionID == value.MissionInfo.MissionID)
                    {
                        m_specialMissions[i].SetActive(true);
                        break;
                    }
                }
                value.Close();
                m_DictTweenerObjects.Remove(id);
                if (m_DictTweenerObjects.Count == 0)
                    m_IsPlayingAnimation = false;
            }
        }

        private int AddAndGetNewMissionPosition(MissionInfo mission)
        {

            if (mission.CheckMissionType(EMissionType.Promoted))
            {
                m_MissionPromoted.Add(mission);
                return 0;
            }

            if (mission.CheckMissionType(EMissionType.Reward))
            {
                m_MissionReward.Add(mission);
                m_MissionReward.Sort(MissionModule.RewardMissionComparer);

                return m_MissionReward.FindIndex(m => m.MissionID == mission.MissionID) + m_MissionPromoted.Count;
            }
            return -1;
        }

        private void CheckEntrustMission(MissionInfo mission)  //委托
        {
            if (!mission.CheckMissionType(EMissionType.Entrust)) { return; }

            var index = m_CurMissions.FindIndex(m => m.MissionID == mission.MissionID);
            ModifyMission(mission);
            if (index == -1)
            {

                return;
            }
            m_CurMissions[index].SetData(mission);
            m_ScrollList.ShowList(GetWidgetCount());
        }

        private void ModifyMission(MissionInfo mission)
        {
            if (!m_DictEntrustMissions.TryGetValue(mission.StrongHoldID, out var datas))
            {
                return;
            }
            var level = mission.Data.DifficultyLevel - 1;
            int index = datas[level].FindIndex(m => m.MissionID == mission.MissionID);
            if (index == -1)
            {
                CLogger.Error("mission not found!");
                return;
            }
            datas[level][index] = mission;
        }

        private void PlayAdventureAudio(string[] audioNames)
        {
            if (null == audioNames || audioNames.Length == 0) { return; }
            var index = Random.Range(0, audioNames.Length);
            if (-1 == index) { return; }
            PlayAudio(audioNames[index]);
        }
    }
}