using Table.Structure;
using UnityEngine;
using ZhFramework.Engine;
using ZhFramework.Unity.Extensions;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{
    public class UnitAdventureReputationIcon : UIWidget
    {
        [Hotfix] private XImage m_ImgStrongholdIcon;
        [Hotfix] private XText m_TxtStrongholdLevel;
        [Hotfix] private XText m_TxtStrongholdName;
        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            var m_stronghold = args[0] as AdventureStronghold;
            var strongholdData = TableExtensions.GetTableStrongholdDescData(m_stronghold.strongholdId);
            var levelupData = TableExtensions.GetStrongholdLevelUpData(m_stronghold.strongholdId, m_stronghold.strongholdLv);
            m_ImgStrongholdIcon.SetSpriteSafe(strongholdData.Icon);
            m_TxtStrongholdLevel.text = TableExtensions.GetStrongholdLevelStyleName(levelupData.Level);
            m_TxtStrongholdName.text = strongholdData.GetStrongholdName();
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


    }
}