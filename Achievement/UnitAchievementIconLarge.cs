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


    public class UnitAchievementIconLarge : UIWidget
    {

        [Hotfix] private XImage m_ImgIcon;
        [Hotfix] private XText m_TxtProgress;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);

        }

        public void SetData(int parent, int id, int tertiary)
        {
            var module = ModuleManager.Instance.Get<AchievementModule>();
            var progress = module.GetAchievementTertiaryTabProgress(new AchievementTabGroup() { First = parent, Secondary = id, Tertiary = tertiary});
            m_TxtProgress.text = string.Format(TableSysPropertyText.Achievement_IconBig_Desc, UIHelper.GetProgressText(progress));
            m_ImgIcon.SetSpriteSafe(UIHelper.GetAchievementSubTabIcon(id,tertiary));
        }
    }
}