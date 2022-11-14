using UnityEngine;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.UI;
using Table.Structure
    ;

namespace Game.Runtime
{
    public class UnitAdventureTaskTab : UIWidget
    {
        [Hotfix] private RectTransform m_RtRoleRoot;
        [Hotfix] private XText m_TxtTaskName;
        [Hotfix] private XText m_TxtTaskType;
        [Hotfix] private XImage m_ImgDifficulties;
        [Hotfix] private XText m_TxtTaskDescription;
        [Hotfix] private XImage m_ImgTaskStatus;
        [Hotfix] private XButton m_Btn;
        [Hotfix] private GameObject m_Completed;
        [Hotfix] private GameObject m_Ongoing;
        [Hotfix] private Transform m_RtMask;

        private MissionInfo m_MissionInfo;
        private UnitRoleHead m_UnitRoleHead;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_UnitRoleHead = CreateWidget<UnitRoleHead>(m_RtRoleRoot);
            m_UnitRoleHead.SetCanClick(false);
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            //m_Btn.onClick.AddListener(OnTabClicked);
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            //m_Btn.onClick.RemoveAllListeners();
        }

        public override void SetData(params object[] args)
        {
            base.SetData(args);
            if (null == args || args.Length == 0) { return; }

            m_MissionInfo = args[0] as MissionInfo;

            m_UnitRoleHead.SetMissionRoleIcon(m_MissionInfo.GetRoleHeadSpriteName());
            m_TxtTaskType.text = TableSysFindText.Find(string.Format("TownDailyTaskType_{0}",m_MissionInfo.Data.SubType));
            m_TxtTaskName.text = MissionExtensions.GetMissionName(m_MissionInfo);
            m_TxtTaskDescription.text = MissionExtensions.GetDescribe(m_MissionInfo);
            m_ImgDifficulties.SetSpriteSafe(MissionExtensions.GetMissionDifficultSpriteName(m_MissionInfo));

            m_RtMask.SetActiveEx(m_MissionInfo.CheckMissionStatus(EMissionStatus.Completed) || m_MissionInfo.CheckAcceptedEntrustMissionInOtherDifficulty());
            m_Completed.SetActiveEx(m_MissionInfo.CheckMissionStatus(EMissionStatus.Completed));
            m_Ongoing.SetActive(!m_MissionInfo.CheckMissionStatus(EMissionStatus.Completed));

            if (!m_MissionInfo.CheckMissionStatus(EMissionStatus.Completed))
            {
                m_ImgTaskStatus.SetSpriteSafe(MissionExtensions.GetAdventureMissionStatusSpriteName(m_MissionInfo));
            }
        }

        private void OnTabClicked()
        {
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}