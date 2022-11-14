using Google.Protobuf;
using Myproto;
using MyProtos;
using System;
using System.Collections.Generic;
using System.Linq;
using Table.Structure;
using UnityEngine;

#pragma warning disable CS0067

namespace Game.Runtime
{
    public class AdventureStronghold
    {
        public int strongholdId;
        public int strongholdLv;
        public long strongholdExp;
        public List<int> strongholdMissions;
        public List<int> strongholdRewardCollected;

        public TableStrongholdDesc.Data strongholdData;
        public int LastPrestigeLevel;                  // 上次据点声望等级
        public int prestigeLevel;                      // 据点声望等级
        public int prestige;                           // 据点声望
        public bool[] rewardFlags;                     // 据点等级领奖情况
        public int currProbability;                    // 当前概率值
        public int maxProbability;                     // 最大概率值
        public List<MissionInfo> missionInfos;         // 当天任务信息
        public MissionInfo rewardTaskInfo;             // 悬赏任务信息
    }

    public class Adventurer
    {
        public long adventurerExp;
        public int adventurerLv;
        public TableAdventurerLevel.Data adventuererLevelData;
        public string SignatureUrl;

        public int dailyCount;
        public int dailyLimit;
        public MissionInfo promotedTaskInfo;          // 晋级任务信息
    }

    public class AdventureManager : Singleton<AdventureManager>
    {
        public event Action onAdventureDataUpdate;                      // 冒险者数据更新

        public event Action onAdventurerDataUpdated;                    // 角色数据更新

        public event Action<int, int> onAdventureLevelUp;               // 冒险者等级升级

        public event Action<int, int, int> onStrongholdLevelUp;

        public event Action<AdventureStronghold> onStrongholdLevelUpNew;

        public event Action<int, long> onStrongholdExpUpNew;

        public event Action<long> onAdventuerExpUpNew;

        public event Action<Adventurer, int> onAdventuerLevelUpNew;

        // 据点升级

        private Dictionary<int, AdventureStronghold> m_AdventureStrongholdDict = new Dictionary<int, AdventureStronghold>(); // 据点Id 数据
        private List<Tuple<int, int>> m_RedDotList = new List<Tuple<int, int>>();
        public List<VoTabData> TabsData = new List<VoTabData>();
        public Dictionary<int, List<int>> RewardDict = new Dictionary<int, List<int>>();
        public Adventurer AdventurerData { private set; get; }

        protected override void Init()
        {
            // 协议
            NetworkManager.Tcp.Regist((int)ID.MyProtosRoleAdventurerExpUpdatedNtf, OnAdventureExpPush);
            NetworkManager.Tcp.Regist((int)ID.MyProtosRoleAdventurerLvUpgradeNtf, OnAdventureLvPush);
            NetworkManager.Tcp.Regist((int)ID.MyProtosRoleAdventurerDataNtf, OnRoleAdventurerDataPush);
            NetworkManager.Tcp.Regist((int)ID.MyProtosRoleStrongholdExpUpdateNtf, OnStrongholdExpPush);
            NetworkManager.Tcp.Regist((int)ID.MyProtosRoleStrongholdLvUpgradeNtf, OnStrongholdLvPush);
            NetworkManager.Tcp.Regist((int)ID.MyProtosRoleStrongholdDataChangedNtf, OnStrongholdDataPush);
            NetworkManager.Tcp.Regist((int)ID.MyProtosRoleStrongholdDataContainerNtf, OnRoleStrongholdDataContainerPush);

        }

        protected override void Dispose()
        {
            TableSysPropertyText.Uninit();

            // 协议
            NetworkManager.Tcp.Unregist((int)ID.MyProtosRoleAdventurerExpUpdatedNtf);
            NetworkManager.Tcp.Unregist((int)ID.MyProtosRoleAdventurerLvUpgradeNtf);
            NetworkManager.Tcp.Unregist((int)ID.MyProtosRoleAdventurerDataNtf);
            NetworkManager.Tcp.Unregist((int)ID.MyProtosRoleStrongholdExpUpdateNtf);
            NetworkManager.Tcp.Unregist((int)ID.MyProtosRoleStrongholdLvUpgradeNtf);
            NetworkManager.Tcp.Unregist((int)ID.MyProtosRoleStrongholdDataChangedNtf);
            NetworkManager.Tcp.Unregist((int)ID.MyProtosRoleStrongholdDataContainerNtf);
        }

        // public

        public List<int> LastUnlockedStrongholdIds = new List<int>();
        public void SetTabData()
        {
            var StrongholdData = GetUnlockedStrongholdData();
            for (int i = 0; i < StrongholdData.Count; i++)
            {
                var strongholdId = StrongholdData[i].strongholdId;
                if (LastUnlockedStrongholdIds.Contains(strongholdId))
                {
                    continue;
                }
                var redDotStronghold = RedDotManager.Instance.AddRedDot((int)RedDotType.Adventure_Reputation, null, false);
                LastUnlockedStrongholdIds.Add(strongholdId);
                TabsData.Add(new VoTabData()
                {
                    ID = strongholdId,
                    RedDotId = redDotStronghold,
                    Name = TableExtensions.GetStrongholdName(StrongholdData[i].strongholdId),
                });
                if(!RewardDict.TryGetValue(strongholdId, out var _))
                {
                    RewardDict[strongholdId] = new List<int>();
                }
                var rewardList = RewardDict[strongholdId] ;
                rewardList.Clear();
                var collected = StrongholdData[i].strongholdRewardCollected;
                for (int j =0; j< TableExtensions.GetStrongholdLevelMax(); j++)
                {
                    var redId = RedDotManager.Instance.AddRedDot(redDotStronghold, null, false);
                    rewardList.Add(redId);
                    if (!collected.Contains(j + 1) && (j+1)<= StrongholdData[i].strongholdLv)
                    {
                        RedDotManager.Instance.SetRedDot(redId, 1);
                    }
                }
                
            }



        }
        public Dictionary<int, AdventureStronghold>.Enumerator GetStrongholdtEnumerator()
        {
            return m_AdventureStrongholdDict.GetEnumerator();
        }

        public AdventureStronghold GetStrongholdData(int strongholdId)
        {

            if (m_AdventureStrongholdDict.ContainsKey(strongholdId))
            {
                return m_AdventureStrongholdDict[strongholdId];
            }

            return new AdventureStronghold()
            {
                strongholdId = strongholdId,
                strongholdExp = 0,
                strongholdLv = 1,
                strongholdMissions = new List<int>() { },
                strongholdRewardCollected = new List<int>() { }
            };
        }

        public List<AdventureStronghold> GetUnlockedStrongholdData()
        {
            var res = new List<AdventureStronghold>();
            var ptr = m_AdventureStrongholdDict.GetEnumerator();
            while (ptr.MoveNext())
            {
                if (ConditionHelper.CheckSysFunc(TableExtensions.GetTableStrongholdDescData(ptr.Current.Value.strongholdId).FuncId))
                {
                    res.Add(ptr.Current.Value);
                }
            }
            return res;
        }

        public List<AdventureStronghold> GetStrongholdTabs()
        {
            var res = new List<AdventureStronghold>();
            var ptr = m_AdventureStrongholdDict.GetEnumerator();
            while (ptr.MoveNext())
            {
                if (ConditionHelper.CheckSysFunc(TableExtensions.GetTableStrongholdDescData(ptr.Current.Value.strongholdId).FuncId) && TableExtensions.GetTableStrongholdDescData(ptr.Current.Value.strongholdId).TabIsOpen)
                {
                    res.Add(ptr.Current.Value);
                }
            }
            ptr.Dispose();
            return res;
        }

        public bool isMagicLandUnlocked()
        {
            return ConditionHelper.CheckSysFunc(TableExtensions.GetTableStrongholdDescData(3).FuncId);
        }

        public int GetMagicLandId()
        {
            return 3;
        }

        public async void GetAdventureReward(int strongholdId)
        {
            VenturerGetRewardREQ req = new VenturerGetRewardREQ()
            {
                StrongholdId = strongholdId
            };
            var result = await req.Invoke<VenturerGetRewardACK>();
            if (result.cancel) return;
            if (result.msg.Ret > 0)
            {
                Debug.Log($"GetAdventureReward ACK {result.msg.Ret}");
            }
        }

        //public List<MissionInfo> GetAllAdventureMission()
        //{
        //    List<MissionInfo> list = new List<MissionInfo>();
        //    var rator = GetStrongholdtEnumerator();
        //    while (rator.MoveNext())
        //    {
        //        var value = rator.Current.Value;
        //        list.AddRange(value.missionInfos);
        //        if (null != value.rewardTaskInfo)
        //            list.Add(value.rewardTaskInfo);
        //    }
        //    rator.Dispose();

        //    if (null != AdventurerData.promotedTaskInfo)
        //        list.Add(AdventurerData.promotedTaskInfo);

        //    return list;
        //}

        /// <summary>
        /// 获取每个据点红点信息
        /// </summary>
        /// <returns>strongholdId, redDotCount</returns>
        public List<Tuple<int, int>> GetRedDotList()
        {
            m_RedDotList.Clear();
            var rator = GetStrongholdtEnumerator();

            while (rator.MoveNext())
            {
                int redDotCount = 0;
                var value = rator.Current.Value;
                if (value.rewardTaskInfo != null)
                    redDotCount++;

                foreach (var taskInfo in value.missionInfos)
                {
                    if (taskInfo.Status == EMissionStatus.CanComplete)
                        redDotCount++;
                }
                Tuple<int, int> redDotData = new Tuple<int, int>(value.strongholdId, redDotCount);
                m_RedDotList.Add(redDotData);
            }

            rator.Dispose();
            return m_RedDotList;
        }

        public bool CheckStrongholdLevelIsValid(int strongholdId, int level)
        {
            //var data = TableExtensions.GetTableStrongholdDescGroupData(strongholdFuncId);
            //if (null == data) { return false; }

            //var strongholdId = data.Id;

            if (!m_AdventureStrongholdDict.TryGetValue(strongholdId, out var advenureStronghold))
            {
                return false;
            }

            return advenureStronghold.strongholdLv >= level;
        }

        public bool CheckAdventurerLevelIsValid(int level)
        {
            if(null == AdventurerData) { return false; }
            return AdventurerData.adventurerLv >= level;
        }

        /// <summary>
        /// 通过任务获取据点Id
        /// </summary>
        /// <returns>据点Id，如果获取不到则返回-1</returns>
        //public int GetStrongholdIdByMission(int missionId)
        //{
        //    var rator = m_AdventureStrongholdDict.GetEnumerator();
        //    int strongholdId = -1;
        //    while (rator.MoveNext())
        //    {
        //        var id = rator.Current.Key;
        //        var val = rator.Current.Value;

        //        var missionInfo = val.missionInfos.Find(f => f.MissionID == missionId);
        //        if (null == missionInfo)
        //            continue;

        //        strongholdId = id;
        //        break;
        //    }
        //    rator.Dispose();
        //    return strongholdId;
        //}

        // private
        private MissionInfo GetMissionInfo(int missionId)
        {
            return ModuleManager.Instance.Get<MissionModule>().GetMissionInfo(missionId);
        }

        #region Protocol

        private void OnAdventureExpPush(IMessage obj)
        {

            var msg = (RoleAdventurerExpUpdatedNtf)obj;

            bool isFirstAdventureData = false;
            // 冒险者信息
            if (AdventurerData == null)
            {
                isFirstAdventureData = true;
                AdventurerData = new Adventurer() { adventurerLv = 1 };
            }

            AdventurerData.adventurerExp = msg.AdventurerExp;
            AdventurerData.adventuererLevelData = TableExtensions.GetTableAdventurerLevelData(AdventurerData.adventurerLv);
            onAdventuerExpUpNew?.Invoke(msg.AdventurerExp);
            //if (!isFirstAdventureData && AdventurerData.ventureLevel > AdventurerData.LastVentureLevel)
            //{
            //    onAdventureLevelUp?.Invoke(AdventurerData.LastVentureLevel, AdventurerData.ventureLevel);
            //}
            //AdventurerData.LastVentureLevel = msg.VenturerLevel;
            //AdventurerData.ventureExp = msg.VenturerExp;
            //AdventurerData.dailyCount = msg.DailyCount;
            //AdventurerData.dailyLimit = msg.DailyLimit;
            //AdventurerData.promotedTaskInfo = GetMissionInfo(msg.PromotedTask);
            //onAdventurerDataUpdated?.Invoke();

            //foreach (var stronghold in msg.Stongholds)
            //{
            //    if (m_AdventureStrongholdDict.ContainsKey(stronghold.StrongholdId))
            //    {
            //        var adventureStronghold = m_AdventureStrongholdDict[stronghold.StrongholdId];
            //        SetStrongholdData(adventureStronghold, stronghold, false);
            //    }
            //    else
            //    {
            //        var adventureStronghold = new AdventureStronghold();
            //        SetStrongholdData(adventureStronghold, stronghold, true);
            //        m_AdventureStrongholdDict.Add(stronghold.StrongholdId, adventureStronghold);
            //    }
            //}

            //onAdventureDataUpdate?.Invoke();
        }

        private void OnAdventureLvPush(IMessage obj)
        {
            var msg = (RoleAdventurerLvUpgradeNtf)obj;

            // 冒险者信息
            if (AdventurerData == null)
            {
                AdventurerData = new Adventurer();
            }

            AdventurerData.adventurerLv = msg.Item.AdventurerLv;
            AdventurerData.adventurerExp = msg.Item.AdventurerExp;
            AdventurerData.adventuererLevelData = TableExtensions.GetTableAdventurerLevelData(AdventurerData.adventurerLv);
            onAdventuerLevelUpNew?.Invoke(AdventurerData, msg.OldAdventurerLv);
            AdventurerData.SignatureUrl = msg.Item.AutographUrl;
        }

        private void OnStrongholdExpPush(IMessage obj)
        {

            var msg = (RoleStrongholdExpUpdateNtf)obj;

            if (m_AdventureStrongholdDict.ContainsKey(msg.StrongholdId))
            {
                m_AdventureStrongholdDict[msg.StrongholdId].strongholdExp = msg.StrongholdExp;
            }
            else
            {
                m_AdventureStrongholdDict.Add(msg.StrongholdId, new AdventureStronghold()
                {
                    strongholdId = msg.StrongholdId,
                    strongholdExp = msg.StrongholdExp,
                    strongholdLv = 1,
                    strongholdMissions = new List<int>() { },
                    strongholdRewardCollected = new List<int>() { }
                });
            }
            onStrongholdExpUpNew?.Invoke(msg.StrongholdId, msg.StrongholdExp);
        }

        private void OnStrongholdLvPush(IMessage obj)
        {
            var msg = (RoleStrongholdLvUpgradeNtf)obj;
            UpdateStrongholdData(msg.Item.StrongholdId, msg.Item.StrongholdExp, msg.Item.StrongholdLv, msg.Item.EntrustTaskGroupIds.ToList<int>(), msg.Item.GaveRewardLevels.ToList<int>());
            onStrongholdLevelUpNew?.Invoke(GetStrongholdData(msg.Item.StrongholdId));
        }

        private void OnStrongholdDataPush(IMessage obj)
        {
            var msg = (RoleStrongholdDataChangedNtf)obj;
            UpdateStrongholdData(msg.Item.StrongholdId, msg.Item.StrongholdExp, msg.Item.StrongholdLv, msg.Item.EntrustTaskGroupIds.ToList<int>(),msg.Item.GaveRewardLevels.ToList<int>());
        }



        private void UpdateStrongholdData(int id, long exp, int level, List<int> missions, List<int> rewards)
        {
            if (m_AdventureStrongholdDict.ContainsKey(id))
            {
                m_AdventureStrongholdDict[id].strongholdExp = exp;
                m_AdventureStrongholdDict[id].strongholdLv = level;
                m_AdventureStrongholdDict[id].strongholdMissions = missions;
                m_AdventureStrongholdDict[id].strongholdRewardCollected = rewards;
            }
            else
            {
                m_AdventureStrongholdDict.Add(id, new AdventureStronghold()
                {
                    strongholdId = id,
                    strongholdExp = exp,
                    strongholdLv = level,
                    strongholdMissions = missions,
                    strongholdRewardCollected = rewards
                });
            }
            SetTabData();
        }

        private void OnRoleAdventurerDataPush(IMessage msg)
        {
            if (!(msg is RoleAdventurerDataNtf)) { return; }

            var push = msg as RoleAdventurerDataNtf;

            AdventurerData = new Adventurer();
            AdventurerData.adventurerExp = push.Data.AdventurerExp;
            AdventurerData.adventurerLv = push.Data.AdventurerLv;
            AdventurerData.adventuererLevelData = TableExtensions.GetTableAdventurerLevelData(AdventurerData.adventurerLv);
            AdventurerData.SignatureUrl = push.Data.AutographUrl;
        }

        private void OnRoleStrongholdDataContainerPush(IMessage msg)
        {
            if (!(msg is RoleStrongholdDataContainerNtf)) { return; }

            var push = msg as RoleStrongholdDataContainerNtf;

            m_AdventureStrongholdDict.Clear();
            var ptr = push.Data.StrongholdData.GetEnumerator();
            while (ptr.MoveNext())
            {
                AdventureStronghold data = new AdventureStronghold()
                {
                    strongholdId = ptr.Current.StrongholdId,
                    strongholdLv = ptr.Current.StrongholdLv,
                    strongholdExp = ptr.Current.StrongholdExp,
                    strongholdMissions = ptr.Current.EntrustTaskGroupIds.ToList<int>(),
                    strongholdRewardCollected = ptr.Current.GaveRewardLevels.ToList<int>()
                    
                };
                m_AdventureStrongholdDict[ptr.Current.StrongholdId] = data;
            }
            ptr.Dispose();
            SetTabData();
        }

        public void ParsePlayerInfo(PlayerInfo info)
        {
            /*AdventurerData = new Adventurer();
            AdventurerData.adventurerExp = info.AdventurerData.AdventurerExp;
            AdventurerData.adventurerLv = info.AdventurerData.AdventurerLv;
            AdventurerData.adventuererLevelData = TableExtensions.GetTableAdventurerLevelData(AdventurerData.adventurerLv);
            m_AdventureStrongholdDict.Clear();
            var ptr = info.StrongholdData.StrongholdData.GetEnumerator();
            while (ptr.MoveNext())
            {
                AdventureStronghold data = new AdventureStronghold(){
                   strongholdId =  ptr.Current.StrongholdId,
                   strongholdLv = ptr.Current.StrongholdLv,
                   strongholdExp = ptr.Current.StrongholdExp,
                   strongholdMissions = ptr.Current.EntrustTaskGroupIds.ToList<int>()
               };
                m_AdventureStrongholdDict[ptr.Current.StrongholdId] = data;
            }
            ptr.Dispose();*/
        }

        #endregion Protocol
    }
}