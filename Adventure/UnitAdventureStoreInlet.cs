using Table.Structure;
using ZhFramework.Unity.UI;

namespace Game.Runtime
{
    public class UnitAdventureStoreInlet : UIWidget
    {
        [Hotfix] private XButton m_btn;
        [Hotfix] private XText m_txt;
        [Hotfix] private XImage m_Img;

        public TableStrongholdDesc.Data m_Data;

        protected override void OnPreload(params object[] args)
        {
            base.OnPreload(args);
            m_Data = args[0] as TableStrongholdDesc.Data;
            m_txt.text = m_Data.GetStrongholdName();
            m_Img.SetSpriteSafe(m_Data.Icon);
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

        private void OnTransferClicked()
        {
            if (null == m_Data) { return; }
            var unit = TableExtensions.GetTableTransferUnitData(m_Data.MapTransfer);
            GameTransfer.Jump(unit);
        }
    }
}