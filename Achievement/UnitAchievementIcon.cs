using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Table.Structure;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using ZhFramework.Engine.Msic;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.Resource;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{


    public class UnitAchievementIcon : UIWidget
    {

        [Hotfix] private XButton m_BtnAction;
        [Hotfix] private XImage m_ImgIcon;
        [Hotfix] private XText m_TxtProgress;
        [Hotfix] private RectTransform m_RootRedDot;
        private int m_ParentId;
        private int m_Id;
        private UnitRedDot m_UnitRedDot;
        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
        }

        public void SetData(int id, int parentid)
        {
            m_ParentId = parentid;
            m_Id = id;
            var module = ModuleManager.Instance.Get<AchievementModule>();
            var tertiary = module.GetPageAchievementInfoDefaultIndex(m_ParentId, m_Id) + 1;
            m_TxtProgress.text = UIHelper.GetAchievementSubTabTitle(id, tertiary,module.GetAchievementTertiaryTabProgress(new AchievementTabGroup() { First = parentid,Secondary = id, Tertiary = tertiary}));
            m_ImgIcon.SetSpriteSafe(UIHelper.GetAchievementSubTabIcon(id, tertiary));
        }

        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            m_BtnAction.onClick.AddListener(OnBtnClicked);
        }

        private void OnBtnClicked()
        {
            UIManager.Instance.CreatePanel<PageAchievementInfo>(m_Id,m_ParentId);
        }

        protected override void UnBindUIEvents()
        {
            base.UnBindUIEvents();
            m_BtnAction.onClick.RemoveAllListeners();
        }

        public void SetRedDot(int redDotId = -1)
        {
            if (redDotId == -1) { return; }
            if (m_UnitRedDot == null)
            {
                m_UnitRedDot = CreateWidget<UnitRedDot>(m_RootRedDot, true, redDotId);
            }
            m_UnitRedDot.SetRedDotNode(redDotId);
        }

    }
}