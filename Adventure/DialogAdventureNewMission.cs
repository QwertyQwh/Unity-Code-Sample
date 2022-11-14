using Table.Structure;
using UnityEngine;
using ZhFramework.UI;
using UnityEngine.UI;
using ZhFramework.Unity.UI;
using System.Collections.Generic;
using System;

namespace Game.Runtime
{
    [UISettings(UILayer.Default, UIContext.Blur | UIContext.Popup | UIContext.Tips)]
    public class DialogAdventureNewMission: CommonDialog_Tips_w
    {
        [Hotfix] private RectTransform m_TaskInfoRoot;
        [Hotfix] private XText m_TxtNewTask;
        public event Action<MissionInfo> onMissionClosed;
        private MissionInfo m_MissionInfo;
        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_MissionInfo = args[0] as MissionInfo;
            if(m_MissionInfo.CheckMissionType(EMissionType.Promoted))
                m_TxtNewTask.text = TableSysPropertyText.AdventurePromotionTask_1;
            else
                m_TxtNewTask.text = TableSysPropertyText.AdventureRewardTask_1;
            CreateWidget<UnitAdventureSpecialTask>(m_TaskInfoRoot, true).SetData(m_MissionInfo);
              
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            onMissionClosed?.Invoke(m_MissionInfo);
        }


    }
}