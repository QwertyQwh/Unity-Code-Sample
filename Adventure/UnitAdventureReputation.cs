using Table.Structure;
using UnityEngine.UI;
using ZhFramework.Unity.UI;
using Myproto;
using MyProtos;
namespace Game.Runtime
{
    public class UnitAdventureReputation : UIWidget
    {
        [Hotfix] private Slider m_slider;
        [Hotfix] private XText m_TxtName;
        [Hotfix] private XImage m_ImgIcon;
        [Hotfix] private XText m_TxtProgress;
        [Hotfix] private XText m_TxtLevel;



        public AdventureStronghold attr;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            attr = args[0] as AdventureStronghold;
            var data = TableExtensions.GetStrongholdLevelUpData(attr.strongholdId, attr.strongholdLv);
            m_slider.maxValue = data.TownExp;
            m_slider.value = attr.strongholdExp;
            m_TxtProgress.text = $"{attr.strongholdExp}/{data.TownExp}";
            PageWaiting.StartWaiting();

            PageWaiting.StopWaiting();

            
            var strongholddata = TableExtensions.GetTableStrongholdDescData(attr.strongholdId);
            m_ImgIcon.SetSpriteSafe(strongholddata.Icon);
            m_TxtName.text = strongholddata.GetStrongholdName();
            m_TxtLevel.text = TableExtensions.GetStrongholdLevelStyleName(data.Level);
            //m_Img.SetSpriteSafe(attr.Icon);
        }


        public override void SetData(params object[] args)
        {
            attr = args[0] as AdventureStronghold;
            var data = TableExtensions.GetStrongholdLevelUpData(attr.strongholdId, attr.strongholdLv);
            m_slider.maxValue = data.TownExp;
            m_slider.value = attr.strongholdExp;
            m_TxtProgress.text = $"{attr.strongholdExp}/{data.TownExp}";


            var strongholddata = TableExtensions.GetTableStrongholdDescData(attr.strongholdId);
            m_ImgIcon.SetSpriteSafe(strongholddata.Icon);
            m_TxtName.text = strongholddata.GetStrongholdName();
            m_TxtLevel.text = TableExtensions.GetStrongholdLevelStyleName(data.Level);
        }
    }
}