using Table.Structure;
using UnityEngine;
using ZhFramework.Engine;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{
    public class UnitAdventureSpecialTask : UIWidget
    {
        [Hotfix] private XButton m_btn;
        [Hotfix] private XText m_TxtCountdown;
        [Hotfix] private XImage m_ImgNewMission;
        [Hotfix] private XImage m_ImgDifficulties;
        [Hotfix] private RectTransform m_RtRoleRoot;
        [Hotfix] private GameObject m_FailedRoot;

        private Timer m_Timer;
        private UnitRoleHead m_UnitRoleHead;
        public MissionInfo MissionInfo;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_UnitRoleHead = CreateWidget<UnitRoleHead>(m_RtRoleRoot);
            m_UnitRoleHead.SetCanClick(false);
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_btn.onClick.AddListener(OnTransferClicked);
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_btn.onClick.RemoveAllListeners();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Timer?.Close();
        }

        public override void SetData(params object[] args)
        {
            base.SetData(args);

            if (null == args || args.Length == 0) { return; }

            MissionInfo = args[0] as MissionInfo;

            m_UnitRoleHead.SetMissionRoleIcon(MissionInfo.GetRoleHeadSpriteName());

            var difficultSpriteName = MissionInfo.GetMissionDifficultSpriteName();
            var isNull = string.IsNullOrEmpty(difficultSpriteName);
            m_ImgDifficulties.SetActiveEx(!isNull);
            if (!isNull)
            {
                m_ImgDifficulties.SetSpriteSafe(difficultSpriteName);
            }

            var spriteName = MissionInfo.GetAdventureMissionRemarkSpriteName(EMissionRemarkType.Entrust_FirstRefresh, out var valid);
            m_ImgNewMission.SetActiveEx(valid);
            if (valid)
            {
                m_ImgNewMission.SetSpriteSafe(spriteName);
            }

            m_FailedRoot.SetActiveEx(MissionInfo.CheckMissionStatus(EMissionStatus.Failed));

            CheckMissionTimeLimit();
        }

        private void CheckMissionTimeLimit()
        {
            m_Timer?.Close();
            m_TxtCountdown.SetActiveEx(MissionInfo.EndTime > 0 && !MissionInfo.IsExpired);
            if (!(MissionInfo.EndTime > 0 && !MissionInfo.IsExpired)) { return; }
            m_TxtCountdown.SetText(MissionInfo.GetMissionCountdownText());
            m_Timer = TimerPool.Start(MissionInfo.Countdown, 1000, OnTimerCallback);
        }

        private void OnTimerCallback(int obj)
        {
            m_TxtCountdown.SetText(MissionInfo.GetMissionCountdownText());
        }

        private void OnTransferClicked()
        {
            if (!MissionInfo.IsValid()) { return; }
            if (MissionInfo.CheckMissionStatus(EMissionStatus.Failed)) { return; }

            if (MissionInfo.CheckMissionStatus(EMissionStatus.Completed))
            {
                PageFloatingTips.ShowFloatingTip(TableSysPropertyText.AdventurerTaskFinish);
            }
            else
            {
                ModuleManager.Instance.Get<MissionModule>().ExecuteAdventurerMission(MissionInfo);
            }
        }
    }
}