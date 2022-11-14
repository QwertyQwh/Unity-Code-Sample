using Table.Structure;
using UnityEngine;
using ZhFramework.UI;
using UnityEngine.UI;
using ZhFramework.Unity.UI;
using System.Collections.Generic;

namespace Game.Runtime
{
    [UISettings(UILayer.Default, UIContext.Blur | UIContext.Popup | UIContext.Tips)]
    public class DialogAdventureReputationLevelUp : CommonDialog_Tips_w
    {


        [Hotfix] private RectTransform m_IconRoot;
        [Hotfix] private RectTransform m_InfoRoot;

        private UnitItemInfo m_ItemData;
        private List<string> m_LevelupDescription;


        private AdventureStronghold m_stronghold;
        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_stronghold = args[0] as AdventureStronghold;
            CreateWidget<UnitAdventureReputationIcon>(m_IconRoot, true, m_stronghold);
            CreateWidget<UnitAdventureReputationLevelUpInfo>(m_InfoRoot, true, m_stronghold);

        }

    }
}