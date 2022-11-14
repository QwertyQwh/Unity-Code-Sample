using Google.Protobuf;
using Myproto;
using MyProtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Table.Structure;
using UnityEngine;
using UnityEngine.Events;
using ZhFramework.Engine;

namespace Game.Runtime
{
    public struct AchievementTabGroup 
    {
        public int First;
        public int Secondary;
        public int Tertiary;
        public override int GetHashCode()
        {
            return (Secondary*100+Tertiary).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is AchievementTabGroup m))
                return false;
            if (Secondary == m.Secondary && Tertiary == m.Tertiary)
                return true;
            return false;
        }

    }

    public class AchievementModule : BaseModule
    {
        public Dictionary<int,int> FirstRedDotIds = new Dictionary<int, int>();
        public Dictionary<int, int> SecondaryRedDotIds = new Dictionary<int, int>();
        public Dictionary<AchievementTabGroup, int> TertiaryRedDotIds = new Dictionary<AchievementTabGroup, int>();
        private Dictionary<AchievementTabGroup, List<AchievementInfo>> m_TabDict = new Dictionary<AchievementTabGroup, List<AchievementInfo>>();
        private Dictionary<AchievementTabGroup, AchievementCollectionInfo> m_CollectionDict = new Dictionary<AchievementTabGroup, AchievementCollectionInfo>();
        public static AchievementInfoComparer AchievementInfoComparer = new AchievementInfoComparer();
        public int PageAchievementLastSelectedIndex = 0;
        public override async void Initialize()
        {
            NetworkManager.Tcp.Regist((int)ID.MyProtosAchievementListNtf, OnAchievementListNtf);
            NetworkManager.Tcp.Regist((int)ID.MyProtosAchievementChangedNtf, OnAchievementChangedNtf);
            InitializeRedDots();
        }


        public void InitializeRedDots()
        {
            var ptr = TableAchievementTab.GetEnumerator();
            while (ptr.MoveNext())
            {
                var data = TableAchievementTab.Find(ptr.Current.Key);
                if (!FirstRedDotIds.ContainsKey(data.Type))
                    FirstRedDotIds[data.Type] = RedDotManager.Instance.AddRedDot((int)RedDotType.Achievement, null, false);
                var secondaryId = RedDotManager.Instance.AddRedDot(FirstRedDotIds[data.Type], null, false);
                SecondaryRedDotIds[data.SubType] = secondaryId;
                foreach(var tab in data.SubTab)
                {
                   TertiaryRedDotIds[new AchievementTabGroup() { First = data.Type, Secondary = data.SubType, Tertiary = tab}] = RedDotManager.Instance.AddRedDot(secondaryId, null, false);
                }
            }
            ptr.Dispose();
        }

        public bool CheckAchievementCollection(AchievementTabGroup groupId, out AchievementCollectionInfo IsComplete)
        {
            if(!m_CollectionDict.TryGetValue(groupId, out var data))
            {
                IsComplete = null;
                return false;
            }
            var reward = data.data.ShowReward;
            if(reward.Length == 0)
            {
                IsComplete = null;
                return false;
            }
            IsComplete = data;
            return true;
        }


        public int GetAchievementSubTabProgress(int id)
        {
            int total = 0;
            int redeemedCount = 0;
            var ptr = m_TabDict.GetEnumerator();
            while (ptr.MoveNext())
            {
                if(ptr.Current.Key.Secondary == id)
                {
                    foreach(var data in ptr.Current.Value)
                    {
                        total++;
                        if (data.Status == EAchievementStatus.Redeemed || data.Status == EAchievementStatus.Redeemable)
                            redeemedCount++;
                    }
                }
            }
            ptr.Dispose();
            if (total == 0)
                return 0;
            return (int)Mathf.Floor( (float)(redeemedCount) / total * 100f);
        }
        public int GetAchievementTertiaryTabProgress(AchievementTabGroup id)
        {
            int total = m_CollectionDict[id].NeedNum;
            int redeemedCount = 0;
            if (m_TabDict.TryGetValue(id, out var values))
            {
                foreach(var val in values)
                {
                    if (val.Status == EAchievementStatus.Redeemable || val.Status == EAchievementStatus.Redeemed)
                        redeemedCount++;
                }
            }
            if (total == 0)
                return 0;
            return Math.Min((int)Mathf.Floor((float)(redeemedCount) / total * 100f),100);
        }

        public int GetPageAchievementInfoDefaultIndex(int first, int secondary)
        {
            var data = TableAchievementTab.Find(secondary,false);
            if (data == null)
            {
                return 0;
            }
            for (int i = 0; i < data.SubTab.Length ; i++)
            {
                if(i == data.SubTab.Length - 1)
                    return i;
                else
                {
                    if(m_CollectionDict.TryGetValue(new AchievementTabGroup() { First = first, Secondary = secondary, Tertiary = i+1}, out var info))
                    {
                        if (!info.IsComplete)
                            return i;
                    }
                }
            }
            return 0;
        }
        public string GetAchievementSubTabProgressText(int id)
        {
            int total = 0;
            int redeemedCount = 0;
            var ptr = m_TabDict.GetEnumerator();
            while (ptr.MoveNext())
            {
                if (ptr.Current.Key.Secondary == id)
                {
                    foreach (var data in ptr.Current.Value)
                    {
                        total++;
                        if (data.Status == EAchievementStatus.Redeemed || data.Status == EAchievementStatus.Redeemable)
                            redeemedCount++;
                    }
                }
            }
            ptr.Dispose();
            if (redeemedCount < total)
                return $"({redeemedCount}/{total})";
            else
                return $"{TableSysPropertyText.Achievement_Complete}";

        }
        public string GetAchievementTertiaryTabProgressText(AchievementTabGroup id)
        {
            int total = m_CollectionDict[id].NeedNum;
            int redeemedCount = 0;
            if (m_TabDict.TryGetValue(id, out var values))
            {
                
                foreach (var val in values)
                {
                    if (val.Status == EAchievementStatus.Redeemable || val.Status == EAchievementStatus.Redeemed)
                        redeemedCount++;
                }
            }
            if (redeemedCount < total)
                return $"({redeemedCount}/{total})";
            else
                return $"{TableSysPropertyText.Achievement_Complete}";
        }

        public bool IsTertiaryTabCompleted(AchievementTabGroup group)
        {
            if(!m_TabDict.TryGetValue(group, out var data))
                return false;
            if (GetAchievementTertiaryTabProgress(group) >= 100)
            {
                return true;
            }
            return false;
        }

        public void UpdateAchievementStatus(int id, EAchievementStatus status)
        {
            var group = TableExtensions.GetAchievementTabGroupFromAchievementId(id);
            if (m_TabDict.TryGetValue(group, out var infos))
            {
                for(int i = 0; i< infos.Count; i++)
                {
                    if(id == infos[i].Id)
                        infos[i].Status = status;
                }
            }
        }

        public void UpdateSubAchievementStatus(AchievementTabGroup group, bool isComplete)
        {
            if (m_CollectionDict.TryGetValue(group, out var infos))
                infos.IsComplete = isComplete;
        }

        public void UpdateRedDots(AchievementTabGroup group)
        {

                int count = 0;

                if (m_TabDict.TryGetValue(group, out var val))
                {
                    foreach (var info in val)
                    {
                        if (info.Status == EAchievementStatus.Redeemable)
                            count++;
                    }
                }
                if (m_CollectionDict.TryGetValue(group, out var data))
                {
                    if (!data.IsComplete && IsTertiaryTabCompleted(group) && data.data.ShowReward.Length !=0)
                        count++;
                }
                RedDotManager.Instance.SetRedDot(TertiaryRedDotIds[group], count);
            
        }
        public void UpdateRedDots()
        {
            var ptr = TertiaryRedDotIds.GetEnumerator();
            while (ptr.MoveNext())
            {

                int count = 0;

                if(m_TabDict.TryGetValue(ptr.Current.Key, out var val))
                {
                    foreach(var info in val)
                    {
                        if(info.Status == EAchievementStatus.Redeemable)
                            count++;
                    }
                }
                if (m_CollectionDict.TryGetValue(ptr.Current.Key, out var data))
                {
                    if (!data.IsComplete && IsTertiaryTabCompleted(ptr.Current.Key)&& data.data.ShowReward.Length !=0)
                        count++;
                }
                RedDotManager.Instance.SetRedDot(ptr.Current.Value, count);
            }
            ptr.Dispose();
        }
        private void OnAchievementChangedNtf(IMessage obj)
        {
            var ntf = (AchievementChangedNtf)obj;
            foreach(var data in ntf.ChangedAchievements)
            {
                var group = TableExtensions.GetAchievementTabGroupFromAchievementId(data.AchievementId);
                if (!m_TabDict.TryGetValue(group, out var infos))
                {
                    m_TabDict[group] = new List<AchievementInfo>();
                    var info = new AchievementInfo(data.AchievementId, (EAchievementStatus)data.State, EAchievementType.Normal,data.Goals[0].CurrentProcess, data.Goals[0].GoalProcess, data.CompletedTime);
                    m_TabDict[group].Add(info);
                    if(info.Status == EAchievementStatus.Redeemable)
                        UnitAchievementReached.ShowFloatingTip(info);
                    continue;
                }
                bool found = false;
                foreach(var info in infos)
                {
                    if(info.Id == data.AchievementId)
                    {
                        info.CompletedDate = data.CompletedTime;
                        info.Status = (EAchievementStatus)data.State;
                        info.Curprogress = data.Goals[0].CurrentProcess;
                        info.TargetProgress = data.Goals[0].GoalProcess;
                        if (info.Status == EAchievementStatus.Redeemable)
                            UnitAchievementReached.ShowFloatingTip(info);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var info = new AchievementInfo(data.AchievementId, (EAchievementStatus)data.State, EAchievementType.Normal, data.Goals[0].CurrentProcess, data.Goals[0].GoalProcess,data.CompletedTime);
                    info.Type = (EAchievementType)info.Data.RecommendType;
                    infos.Add(info);
                    if (info.Status == EAchievementStatus.Redeemable)
                        UnitAchievementReached.ShowFloatingTip(info);
                }
            }

        }

        public static void RequestAchievementInfo()
        {
            var req = new AchievementListReq();
            req.Invoke();
        }

        public async void SubmitAchievement(int m_id)
        {
            PageWaiting.StartWaiting();
            var req = new SubmitAchievementReq()
            {
                AchievementId = m_id,
            };

            var ack = await req.Invoke<SubmitAchievementAck>();

            UIHelper.OpenDialogCommonReward(ack.msg.RewardItem);

            UpdateAchievementStatus(m_id, EAchievementStatus.Redeemed);
            UpdateRedDots(TableExtensions.GetAchievementTabGroupFromAchievementId(m_id));
            PageWaiting.StopWaiting();
        }

        public async void SubmitSubAchievement(AchievementCollectionInfo info)
        {
            PageWaiting.StartWaiting();
            var req = new ActivateSubAchievementReq()
            {
                Id = info.Id,
            };
            var ack = await req.Invoke<ActivateSubAchievementAck>();

            UIHelper.OpenDialogCommonReward(ack.msg.RewardItem);

            UpdateSubAchievementStatus(info.TabId, true);
            UpdateRedDots();

            PageWaiting.StopWaiting();
        }

        public async void SubmitSecondaryAllAchievements(AchievementCollectionInfo info)
        {
            PageWaiting.StartWaiting();
            var req = new PickUpOneButtonReq()
            {
                Id = info.Id,
            };
            var ack = await req.Invoke<PickUpOneButtonAck>();

            UIHelper.OpenDialogCommonReward(ack.msg.RewardItem);

            UpdateSubAchievementStatus(info.TabId, true);
            UpdateRedDots();

            PageWaiting.StopWaiting();
        }

        public List<AchievementInfo> GetAchievementInfosFromTabGroup(AchievementTabGroup groupId)
        {
            if (m_TabDict.TryGetValue(groupId, out var achievementInfos))
                return achievementInfos;
            else
                return new List<AchievementInfo>();
        }
        private void OnAchievementListNtf(IMessage obj)
        {
            var ntf = (AchievementListNtf)obj;
            m_TabDict.Clear();
            foreach(var achievement in ntf.Achievements)
            {
                var key = TableExtensions.GetAchievementTabGroupFromAchievementId(achievement.AchievementId);
                var val = new AchievementInfo(achievement.AchievementId,
                    (EAchievementStatus)achievement.State,
                    EAchievementType.Normal, achievement.Goals[0].CurrentProcess, achievement.Goals[0].GoalProcess,achievement.CompletedTime);
                val.Type = (EAchievementType)val.Data.RecommendType;
                if (!m_TabDict.ContainsKey(key))
                    m_TabDict[key] = new List<AchievementInfo>();
                m_TabDict[key].Add(val);
            }
            m_CollectionDict.Clear();
            foreach(var collection in ntf.SubAchievements)
            {
                var group = TableExtensions.GetAchievementTabGroupFromGroupId(collection.Id);
                m_CollectionDict[group] = new AchievementCollectionInfo(collection.Id,
                    group,
                    collection.State,
                    collection.NeedNum,
                    collection.CompletedTime
                    );
            }
        }
        public override void Release()
        {
            NetworkManager.Tcp.Unregist((int)ID.MyProtosAchievementListNtf);
            NetworkManager.Tcp.Unregist((int)ID.MyProtosAchievementChangedNtf);

        }
    }

}