using System;
using UnityEngine;
using UnityEngine.UI;
using ZhFramework.Unity.Resource;
using ZhFramework.Unity.UI;
using Table.Structure;


namespace Game.Runtime
{
    public class UnitAdventureReputationTab : UIWidget
    {
        [Hotfix] private XText m_TxtLevelupTitle;
        [Hotfix] private RectTransform m_LevelupDescriptionRoot;
        [Hotfix] private XText m_TxtReputation;


        

        public TableStrongholdLevelUp.Data LevelupAttr;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            LevelupAttr = args[0] as TableStrongholdLevelUp.Data; 
            m_TxtLevelupTitle.text = string.Format(TableSysPropertyText.StrongholdReward,  TableExtensions.GetStrongholdName(LevelupAttr.TownId), LevelupAttr.Level);
            CreateWidget<UnitStrongholdPrestigeUnlockItem>(m_LevelupDescriptionRoot,true, TableExtensions.GetStrongholdLevelConditions(TableExtensions.GetStrongholdLevelUpData(LevelupAttr.TownId, LevelupAttr.Level))[0]);
            m_TxtReputation.text = TableExtensions.GetStrongholdLevelStyleName(TableExtensions.GetStrongholdLevelUpData(LevelupAttr.TownId, LevelupAttr.Level).Level);
        }







    }
}
