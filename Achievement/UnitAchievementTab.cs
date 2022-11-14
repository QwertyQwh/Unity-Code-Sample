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


    public class UnitAchievementTab : UnitTab
    {
        #region Feilds
        [Hotfix] private XImage m_ImgIcon;
        [Hotfix] private XText m_txtTitle;

        #endregion Feilds
        


        #region Methods

        public void SetIconAndText(string icon, string txt )
        {
            m_ImgIcon.SetSpriteSafe(icon);
            m_txtTitle.text = txt;
        }

        public override void SetIndex(int value)
        {
            base.SetIndex(value);
            m_ImgIcon.SetSpriteSafe(UIHelper.GetAchievementTabIcon(value+1));
        }



        #endregion Methods
    }
}