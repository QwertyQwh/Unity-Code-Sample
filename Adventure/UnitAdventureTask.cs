//using Table.Structure;
//using UnityEngine;
//using UnityEngine.UI;
//using ZhFramework.Unity.UI;

//namespace Game.Runtime
//{
//    public class AdventureMissionInfo
//    {
//        public MissionInfo info;         // 任务信息
//        public bool hasProgress;         // 有悬赏概率
//        public string progress;          // 悬赏概率
//        public bool hasLevelupEntryAnim; // 有任务进入动画
//    }

//    /// <summary>
//    /// 如果Info为null 则为 未接到任务的悬赏任务
//    /// </summary>
//    public class UnitAdventureTask : UIWidget
//    {
//        [Hotfix] private GameObject m_StatusRoot;        // 状态根
//        [Hotfix] private XImage m_TaskStatusImg;         // 任务状态图
//        [Hotfix] private XText m_TaskStatusText;         // 任务状态
//        [Hotfix] private GameObject m_TaskCompleteRoot;  // 任务完成状态根
//        [Hotfix] private XText m_TaskCompleteText;       // 任务完成文本
//        [Hotfix] private XImage m_NormalTaskImg;         // 普通任务类型图片
//        [Hotfix] private XImage m_OtherTaskImg;          // 悬赏、晋级等任务类型图片
//        [Hotfix] private XText m_OtherTaskText;          // 悬赏、晋级等任务类型文本
//        [Hotfix] private Transform m_TaskHeadContent;    // 任务头像挂点
//        [Hotfix] private XText m_ProbabilityText;        // 悬赏任务概率文本
//        [Hotfix] private Slider m_ProbabilitySlider;     // 概率进度
//        [Hotfix] private XText m_TaskName;               // 任务名称文本
//        [Hotfix] private XButton m_TaskDetailBtn;        // 任务详情按钮
//        [Hotfix] private Animator m_LevelUpTaskAnimator; // 晋级任务特效
//        [Hotfix] private XImage m_RewardStateImg;        // 奖励领取状态图

//        [Hotfix] private Sprite m_RewardImgClose; // 奖励未领取
//        [Hotfix] private Sprite m_RewardImgOpen;  // 奖励已领取

//        private UnitRoleHead m_UnitRoleHead;

//        public AdventureMissionInfo missionInfo;

//        protected override void OnPreload(params object[] args)
//        {
//            base.OnPreload(args);
//            m_UnitRoleHead = CreateWidget<UnitRoleHead>(m_TaskHeadContent);
//        }

//        public override void SetData(params object[] args)
//        {
//            base.SetData(args);

//            if (null == missionInfo)
//                missionInfo = args[0] as AdventureMissionInfo;

//            m_UnitRoleHead.SetMissionRoleIcon(null == missionInfo.info ? TableGameAsset.AdventureQuestion : missionInfo.info.GetRoleHeadSpriteName());

//            m_ProbabilityText.gameObject.SetActive(missionInfo.hasProgress);
//            m_ProbabilitySlider.gameObject.SetActive(missionInfo.hasProgress);

//            m_StatusRoot.SetActive(missionInfo.info != null && !missionInfo.info.CheckMissionStatus(EMissionStatus.Acceptable));
//            m_TaskCompleteRoot.SetActive(missionInfo.info != null && missionInfo.info.Status == EMissionStatus.Completed);
//            m_TaskStatusText.transform.parent.gameObject.SetActive(missionInfo.info != null && missionInfo.info.Status != EMissionStatus.Completed);
//            // 难度 或者 类型 Img
//            m_NormalTaskImg.transform.parent.gameObject.SetActive(missionInfo.info != null && missionInfo.info.Data.Type == (int)EMissionType.Entrust);
//            m_OtherTaskImg.transform.parent.gameObject.SetActive(missionInfo.info == null || missionInfo.info.Data.Type != (int)EMissionType.Entrust);

//            m_TaskName.transform.parent.gameObject.SetActive(missionInfo.info != null);
//            m_ProbabilitySlider.gameObject.SetActive(missionInfo.info == null);

//            m_LevelUpTaskAnimator.gameObject.SetActive(missionInfo.info != null && missionInfo.info.Data.Type == (int)EMissionType.Promoted);

//            if (null != missionInfo.info)
//            {
//                m_TaskStatusImg.gameObject.SetActive(!missionInfo.info.CheckMissionStatus(EMissionStatus.CanComplete));
//                if (missionInfo.info.CheckMissionStatus(EMissionStatus.Accepted))
//                    m_TaskStatusText.textStyle = XStyleSheet.GetBLUE2TextStyle();
//                else
//                    m_TaskStatusText.textStyle = XStyleSheet.GetY2TextStyle();

//                m_TaskCompleteText.text = missionInfo.info.GetStatusText();
//                m_TaskStatusText.text = missionInfo.info.GetStatusText();

//                m_NormalTaskImg.SetSprite(missionInfo.info.GetMissionDifficultSpriteName());
//                m_OtherTaskImg.ColorStyle = missionInfo.info.GetStatusColorStyle().hashCode;
//                m_OtherTaskText.text = missionInfo.info.GetMissionTypeText();
//                m_TaskName.SetText(missionInfo.info.Name);

//                if (!missionInfo.hasLevelupEntryAnim)
//                {
//                    m_LevelUpTaskAnimator.SetTrigger("setloop");
//                }

//                // 设置奖励状态
//                m_RewardStateImg.gameObject.SetActive(true);
//                m_RewardStateImg.sprite = (missionInfo.info.Status == EMissionStatus.Completed) ? m_RewardImgOpen : m_RewardImgClose;
//            }
//            else
//            {
//                m_OtherTaskImg.ColorStyle = MissionExtensions.GetStatusColorStyle((int)EMissionType.Entrust).hashCode;
//                m_OtherTaskText.text = MissionExtensions.GetMissionTypeText((int)EMissionType.Entrust);
//                float progress = float.Parse(missionInfo.progress);
//                m_ProbabilityText.text = $"{TableSysPropertyText.AdventurerProbability}{progress * 100}%";
//                m_ProbabilitySlider.value = progress;

//                m_RewardStateImg.gameObject.SetActive(false);
//            }
//        }

//        protected override void BindUIEvents()
//        {
//            base.BindUIEvents();
//            m_TaskDetailBtn.onClick.AddListener(OnTaskDetailClicekd);
//        }

//        protected override void UnBindUIEvents()
//        {
//            base.UnBindUIEvents();
//            m_TaskDetailBtn.onClick.RemoveAllListeners();
//        }

//        private void OnTaskDetailClicekd()
//        {
//            if (missionInfo.info != null)
//            {
//                if (missionInfo.info.Status == EMissionStatus.Completed)
//                {
//                    PageFloatingTips.ShowFloatingTip(TableSysPropertyText.AdventurerTaskFinish);
//                }
//                else
//                {
//                    ModuleManager.Instance.Get<MissionModule>().ExecuteAdventurerMission(missionInfo.info);
//                }
//            }
//            else
//            {
//                PageFloatingTips.ShowFloatingTip(TableSysPropertyText.AdventureNoRewardTask);
//            }
//        }
//    }
//}