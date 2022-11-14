using System;
using System.Collections.Generic;
using Table.Structure;
using ZhFramework.Engine.Msic;

namespace Game.Runtime
{


    public class AchievementInfo
    {
        public int Id;
        public EAchievementStatus Status;
        public EAchievementType Type;
        public TableAchievement.Data Data;
        public int Curprogress;
        public int TargetProgress;
        public long CompletedDate;
        public AchievementInfo(int id, EAchievementStatus status, EAchievementType type, int cur, int tar, long date)
        {
            Id = id;
            Status = status;
            Type = type;
            Data = TableAchievement.Find(id, false);
            Curprogress = cur;
            TargetProgress = tar;
            CompletedDate = date;
            if(Data == null)
            {
                CLogger.Error($"TableAchievement未找到{id}");
            }
        }

    }


    
    public class AchievementCollectionInfo
    {
        public int Id;
        public AchievementTabGroup TabId;
        public bool IsComplete;
        public TableAchievementStageReward.Data data;
        public long CompletedDate;
        public int NeedNum;

        public AchievementCollectionInfo(int id, AchievementTabGroup tab, bool complete, int needNum, long date)
        {
            Id = id;
            TabId = tab;
            IsComplete = complete;
            data = TableAchievementStageReward.Find(id, false);
            NeedNum = needNum;
            CompletedDate = date;
            if(data == null)
            {
                CLogger.Error($"TableAchievementStageReward未找到{id}");
            }
        }
    }

    public abstract class BaseAchievementInfoComparer : IComparer<AchievementInfo>
    {
        protected int CompareID(AchievementInfo x, AchievementInfo y)
        {
            return x.Id.CompareTo(y.Id);
        }

        //protected int CompareType(AchievementInfo x, AchievementInfo y)
        //{
        //    return x.Type.CompareTo(y.Data.Type);
        //}

        protected int CompareStatus(AchievementInfo x, AchievementInfo y)
        {
            if(x.Status == y.Status)
            {
                return x.Id.CompareTo(y.Id);
            }
            if(x.Status == EAchievementStatus.Redeemed)
            {
                return 1;
            }
            if(y.Status == EAchievementStatus.Redeemed)
            {
                return -1;
            }
            var res = -x.Status.CompareTo(y.Status);
            return  res == 0? x.Id.CompareTo(y.Id): res;
        }

        protected int CompareType(AchievementInfo x, AchievementInfo y)
        {
            if (x.Type == y.Type)
            {
                return x.Id.CompareTo(y.Id);
            }
            var res = -x.Type.CompareTo(y.Type);
            return res == 0 ? x.Id.CompareTo(y.Id) : res;
        }

        /// <summary>
        /// 根据给出的比较顺序排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="compareFuns"></param>
        /// <returns></returns>
        //protected int Sort(AchievementInfo a, AchievementInfo b, Func<AchievementInfo, AchievementInfo, int>[] compareFuns)
        //{
        //    int result = 0;
        //    for (int i = 0; i < compareFuns.Length; i++)
        //    {
        //        result = compareFuns[i].Invoke(a, b);
        //        if (result != 0)
        //        {
        //            return result;
        //        }
        //    }
        //    return result;
        //}

        public virtual int Compare(AchievementInfo a, AchievementInfo b)
        {
            //Func<MissionInfo, MissionInfo, int>[] compareFuns = {
            //    CompareType,
            //    CompareStatus,
            //    CompareID,
            //};
            //return Sort(x, y, compareFuns);
            return 0;
        }
    }
    public class AchievementInfoComparer : BaseAchievementInfoComparer
    {
        public override int Compare(AchievementInfo x, AchievementInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            
            if(x.Status != y.Status)
            {
                return CompareStatus(x, y);
            }
            else
            {
                return CompareType(x, y);
            }
        }
    }

}