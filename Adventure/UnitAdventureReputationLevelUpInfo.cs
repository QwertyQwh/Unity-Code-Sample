using Table.Structure;
using UnityEngine;
using ZhFramework.Engine;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.UI;
using System.Collections.Generic;

namespace Game.Runtime
{
    public class UnitAdventureReputationLevelUpInfo : UIWidget
    {
        [Hotfix] private XScrollList m_ScrollList;
        [Hotfix] private XText m_TxtReward;
        [Hotfix] private XText m_TxtDescription;
        private UnitItemInfo m_ItemData;
        private List<string> m_LevelupDescription;
        private AdventureStronghold m_stronghold;
        [Hotfix] private RectTransform m_ItemInfoRoot;
        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_stronghold = args[0] as AdventureStronghold;
            m_TxtReward.text = TableSysPropertyText.StrongholdLevelupReward;
            m_TxtDescription.text = TableSysPropertyText.StrongholdLevelupDescription;
            var levelupData = TableExtensions.GetStrongholdLevelUpData(m_stronghold.strongholdId, m_stronghold.strongholdLv);


            if (levelupData.Reward.Length == 0)
            {
                Debug.LogError($"据点没有奖励");
            }
            else
            {
                m_ItemData = CreateWidget<UnitItemInfo>(m_ItemInfoRoot, true, levelupData.Reward[0][0], levelupData.Reward[0][1]);
            }

            m_LevelupDescription = TableExtensions.GetStrongholdLevelConditions(levelupData);
            m_ScrollList.Init(CreateWidget, ShowWidget, ClickWidget);
            m_ScrollList.ShowList(GetWidgetCount());
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        private UnitStrongholdPrestigeUnlockItem CreateWidget()
        {
            return CreateWidget<UnitStrongholdPrestigeUnlockItem>(m_ScrollList.content, true, string.Empty);
        }

        private void ClickWidget(int index, object obj)
        {
        }

        private void ShowWidget(int index, UnitStrongholdPrestigeUnlockItem widget)
        {
            var data = GetWidgetData(index);
            if (data == null) { return; }
            widget.SetData(data);
        }
        private string GetWidgetData(int index)
        {
            if (null == m_LevelupDescription) { return null; }
            if (index < 0 || index >= m_LevelupDescription.Count)
            {
                return null;
            }
            return m_LevelupDescription[index];
        }

        private int GetWidgetCount()
        {
            return (null == m_LevelupDescription ? 0 : m_LevelupDescription.Count);
        }

    }
}